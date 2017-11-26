using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Loading_a_3D_model.FileFormats.Images
{
    public class ObeImage
    {
        public int Width;
        public int Height;
        public ushort StartIndex;
        public uint[] Palatte;
        public byte[] Bytes;
    }

    public class ObeCompression // Paletted Indexed Compression
    {
        private enum CompCodes : byte
        {
            NoChangeX = 0,
            ByteX = 1,
            ShortX = 2,
            ShortOne = 3,
        }

        private class CompCodeStruct
        {
            public CompCodes CompCode;
            public Int32 Data;
            public Int32 Rle;

            public CompCodeStruct(CompCodes code, Int32 data, Int32 rle) {
                CompCode = code;
                Data = data;
                Rle = rle;
            }
        }
        public static Texture2D ReadFromFile(GraphicsDevice graphicsDevice, String filename)
        {
            // LoadFile
            var image = new ObeImage();
            using(var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using(var br=new BinaryReader(fs))
                {
                    br.ReadInt32(); // OBE0
                    var w = br.ReadInt32();
                    var h = br.ReadInt32();
                    var s = br.ReadUInt16();
                    var pl = br.ReadInt32();
                    image.Width = w;
                    image.Height = h;
                    image.StartIndex = (ushort)s;
                    image.Palatte = new uint[pl];
                    for(int i=0;i<pl;i++) {
                        image.Palatte[i] = br.ReadUInt32();
                    }

                    var bl = br.ReadInt32();
                    image.Bytes = br.ReadBytes(bl);
                }
            }

            // Decompress
            return Decode(graphicsDevice, image);
        }

        public static void WriteToFile(GraphicsDevice graphicsDevice, Texture2D texture, String filename) {
            var image = Compress(graphicsDevice, texture);

             using(var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
             {
                 using(var bw = new BinaryWriter(fs))
                 {
                     bw.Write((byte)79);bw.Write((byte)66);bw.Write((byte)69);bw.Write((byte)(0));
                     bw.Write((int)image.Width);
                     bw.Write((int)image.Height);
                     bw.Write((ushort)image.StartIndex);
                     bw.Write((int)image.Palatte.Length);
                     for(int i=0;i<image.Palatte.Length;i++)
                        bw.Write(image.Palatte[i]);
                     bw.Write((int)image.Bytes.Length);
                     for(int i=0;i<image.Bytes.Length;i++)
                        bw.Write(image.Bytes[i]);
                 }
             }
        }

        public static ObeImage Compress(GraphicsDevice device, Texture2D texture) {

            var colorData = new uint[texture.Width*texture.Height];
            texture.GetData(colorData);

            var rv = new ObeImage {Width = texture.Width, Height = texture.Height};

            var colorCount = new Dictionary<uint, uint>();
            {
                for(int i=0;i<colorData.Length; i++)
                {
                    if (!colorCount.ContainsKey(colorData[i]))
                    {
                        colorCount.Add(colorData[i], 1);
                    }
                    else
                    {
                        var v = colorCount[colorData[i]];
                        v++;
                        colorCount[colorData[i]] = v;
                    }
                }
            }
            var results = colorCount.ToList();
            results.Sort((a,b) => a.Value <= b.Value ? 1 : -1);


            var paletteDictionary = new Dictionary<uint,uint>();
            var pallete = new uint[results.Count];
            {
                var i=0;
                foreach (var keyValuePair in results)
                {
                    pallete[i] = keyValuePair.Key;
                    
                    paletteDictionary.Add(pallete[i], (uint)i);
                    i++;
                }
            }
            var indexedData = new uint[texture.Width*texture.Height];
            {
                for(int i=0;i<colorData.Length;i++)
                {
                    var c = colorData[i];
                    indexedData[i] = paletteDictionary[c];
                }
            }

            var diffIndex = new int[texture.Width*texture.Height];
            {
                for(int i=1;i<diffIndex.Length;i++)
                {
                    var a = (int) indexedData[i];
                    var b = (int) indexedData[i-1];
                    diffIndex[i-1] =  a - b;
                }
            }

            var opcodes = new List<CompCodeStruct>();
            long current = indexedData[0];
            for(var i=0;i<diffIndex.Length;i++)
            {
                // Output the opcodes
                var v = diffIndex[i];
                current+=(int)v;
                if (v==0) {
                    opcodes.Add(new CompCodeStruct(CompCodes.NoChangeX, v, 0));
                    continue;
                }
                if (v>=-128&&v<=127)
                {
                    opcodes.Add(new CompCodeStruct(CompCodes.ByteX, v, 0));
                    continue;
                }

                // Shorts are literals
                var l = (int)indexedData[i];
                opcodes.Add(new CompCodeStruct(CompCodes.ShortX, (int)current, 0));
            }

            
            {
                for(int opCodeIndex=0; opCodeIndex<opcodes.Count; )
                {
                    var rle = (byte) 1;
                    for(var j=opCodeIndex+1;j<opcodes.Count && rle < 32; j++)
                    {
                        var nextOp = opcodes[j].CompCode;

                        if (nextOp == opcodes[opCodeIndex].CompCode) {
                            rle++;
                        } else {
                            break;
                        }
                    }
                    if (opcodes[opCodeIndex].CompCode == CompCodes.ShortX)
                    {
                        if (rle==1)
                        {
                            opcodes[opCodeIndex].CompCode = CompCodes.ShortOne;
                        }
                    }
                    opcodes[opCodeIndex].Rle = rle;
                    opCodeIndex+=rle;
                }
            }

            var output = new byte[indexedData.Length*4];    // Lets just make this big in case we've made the filesize bigger!
            var startIndex = indexedData[0];
            var outputIndex=0;
            {
                for(int opCodeIndex=0; opCodeIndex<opcodes.Count; )
                {
                    switch (opcodes[opCodeIndex].CompCode)
                    {
                      case CompCodes.NoChangeX:
                          {
                            var opNibble = ((byte) opcodes[opCodeIndex].CompCode);
                            var rle = opcodes[opCodeIndex].Rle;
                            var rleNibble = (byte) ((rle-1)<< 3);

                            if (rle == 0)
                            {
                                throw new Exception("Unexpected rle 0");
                            }

                            var writeByte =  (byte)(opNibble | rleNibble);
                            output[outputIndex] = writeByte;
                            outputIndex++;

                            opCodeIndex+=rle;
                            // No data needs to be written as the RLE count is all we need in this case
                            break;
                          }
                          case CompCodes.ByteX:
                          {
                              var offset=128;

                              // Write opcode and RLE value
                              var opNibble = ((byte) opcodes[opCodeIndex].CompCode);
                              var rle = (opcodes[opCodeIndex].Rle);
                              var rleNibble = (byte) ((rle-1) << 3);
                              var writeByte =  (byte)(opNibble | rleNibble);
                              output[outputIndex] = writeByte;

                              outputIndex++;
                              for(int k=0;k<rle;k++)
                              {
                                  // Write data 1 byte per RLE count
                                  var currentData = (byte)(opcodes[opCodeIndex].Data+offset);
                                  output[outputIndex] = currentData;
                                  outputIndex++;
                                  opCodeIndex++;
                              }
                            break;
                          }
                          case CompCodes.ShortOne:
                          case CompCodes.ShortX: // TODO More cases to minimise shortX
                          {
                              // Write opcode and RLE value
                              var opNibble = ((byte) opcodes[opCodeIndex].CompCode);
                              var rle = opcodes[opCodeIndex].Rle;
                              var rleNibble = (byte) ((rle-1) << 3);
                              var writeByte =  (byte)(opNibble | rleNibble);
                              output[outputIndex] = writeByte;

                              outputIndex++;
                              
                              for(int k=0;k<rle;k++)
                              {
                                  // Write data (2bytes)
                                  var currentData = (short)(opcodes[opCodeIndex].Data);

                                  if (opcodes[opCodeIndex].Data < 0)
                                  {
                                      throw new Exception("Unexpected value");
                                  }

                                  // Low byte
                                  // High byte
                                  output[outputIndex] = (byte) (currentData >> 0);
                                  output[outputIndex+1] = (byte) (currentData >> 8);

                                  outputIndex+=2;
                                  opCodeIndex++;
                              }
                            break;
                          }
                        default:
                        {
                            throw  new Exception("Unknown comp code");
                        }
                    }
                }
            }

            Console.WriteLine("Length = " + (outputIndex+1) + " bytes");
            //Console.WriteLine("");
            // TODO Test decompression

            rv.Bytes = new byte[outputIndex];
            for(int i=0;i<outputIndex;i++)
            {
                rv.Bytes[i] = output[i];
            }
            rv.Palatte = pallete;
            rv.StartIndex = (ushort)startIndex;

            return rv;
        }

        public static unsafe Texture2D Decode(GraphicsDevice device, ObeImage pic) {
            var colors = new uint[pic.Width*pic.Height];
            var palette = pic.Palatte;
            var output = 0;
            var currentIndex = (int) pic.StartIndex;
            colors[0] = palette[currentIndex];
            
            fixed (uint* dstp = &colors[0])
            fixed(byte* bp = &pic.Bytes[0]) 
            {
                var bpoint = bp;
                for(int input=0;input<pic.Bytes.Count(); )
                {
                    var decode = (CompCodes)(pic.Bytes[input] & 0x07);
                    switch (decode)
                    {
                        case CompCodes.NoChangeX:
                            {
                                // Write current color x times
                                var rle = (byte) (pic.Bytes[input] >> 3)+1;
                                input++;
                                
                                var color = palette[currentIndex];

                                if (rle%4==0) 
                                {
                                    for(var j=0;j<rle;j+=4) {
                                        colors[output+0] = color;
                                        colors[output+1] = color;
                                        colors[output+2] = color;
                                        colors[output+3] = color;
                                        output+=4;
                                    }
                                } 
                                else 
                                {
                                    var r = rle%2;
                                    {
                                        for(var j=0;j<rle-1;j+=2) {
                                            colors[output] = color;
                                            colors[output+1] = color;
                                            output+=2;
                                        }
                                    }

                                    if (r==1)
                                    {
                                        colors[output] = color;
                                        output++;
                                    }
                                }

                                break;
                            }
                        case CompCodes.ByteX:
                            {
                                var rle = (byte) (pic.Bytes[input] >> 3)+1;
                                input++;

                                for(var i=0;i<rle;i++)
                                {
                                    var jump = (pic.Bytes[input] - 128);
                                    currentIndex += jump;
                                    colors[output] = palette[currentIndex];
                                    output++;
                                    input++;
                                }
                                break;
                            }
                        case CompCodes.ShortOne:
                        {
                            input++;
                            var a = *((short*) (bpoint + input));
                            colors[output] = palette[a];
                            currentIndex = a;
                            input+=2;
                            output++;
                            break;
                        }
                        case CompCodes.ShortX:
                            {
                                var rle = (byte) (pic.Bytes[input] >> 3)+1;
                                input++;

                                var r = rle%2;
                                {
                                    for(var j=0;j<rle-1;j+=2) {
                                        var a = *((short*) (bpoint + input));
                                        colors[output] = palette[a];

                                        var b = *((short*) (bpoint + input+2));
                                        colors[output+1] = palette[b];

                                        currentIndex = b;
                                        output+=2;
                                        input+=4;
                                    }
                                }

                                if (r==1)
                                {
                                    currentIndex = *((short*) (bpoint + input));
                                    colors[output] = palette[currentIndex];
                                    output++;
                                    input+=2;
                                }
                                break;
                            }
                        default:
                        {
                            throw new Exception("Unexpected CompCode: " + decode + " at index " + input);
                        }
                    }
                }
            }
                       

            var rv = new Texture2D(device, pic.Width, pic.Height);
            rv.SetData(colors);

            return rv;
        }
    }
}
