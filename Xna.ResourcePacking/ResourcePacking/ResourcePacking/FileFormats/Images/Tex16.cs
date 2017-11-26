using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Loading_a_3D_model.Pack;
using Microsoft.Xna.Framework.Graphics;

namespace Loading_a_3D_model.FileFormats.Images
{
    public class Tex16
    {
         public static void WriteToFile(GraphicsDevice device, Texture2D image, string filename) {
             
             var w = image.Width;
             var h = image.Height;

             var palette = GetPalette(image);
             var colorData = new uint[image.Width*image.Height];
             image.GetData(colorData);

             var output = new ushort[image.Width*image.Height];
             for(int i=0;i<colorData.Length;i++)
             {
                 output[i] = palette[colorData[i]];
             }

             var paletteArray = palette.Keys.ToArray();
             using(var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
             {
                 using(var bw = new BinaryWriter(fs))
                 {
                     bw.Write((Int32)16);
                     bw.Write((Int32)w);
                     bw.Write((Int32)h);
                     bw.Write((Int32)paletteArray.Length);
                     for(int i=0;i<paletteArray.Length;i++) 
                        bw.Write(paletteArray[i]);

                     for(int i=0;i<output.Length;i++) 
                        bw.Write(output[i]);
                 }
             }
         }


        public static unsafe Texture2D ReadFromFile(GraphicsDevice device, string filename) {
            var fr = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            var rv = DecodeStream(device, fr);
             fr.Close();
            return rv;
        }

        public static unsafe Texture2D DecodeFromBytes(GraphicsDevice device, byte[] ms, int offset) {
            fixed (byte* fp = &ms[offset])
            {
                var pp = (uint*) fp;        // Palette Pointer
                var ip = (ushort*) fp;      // Indicies Pointer

                var mn =*(int*) fp;
                var w = *(int*) (fp+4);
                var h = *(int*) (fp+8);
                var pl = *(int*) (fp+12);   // Palette Length

                pp = (uint*) (fp+16);        // Start of the palette segment
                ip = (ushort*) (fp+20+pl*4);

                var colors = new uint[w*h];
                {
                    for (int i = 0; i < w*h; i++)
                    {
                        var index = *((ip) + i);
                        var color = *((pp) + index);

                        colors[i] = color;
                    }
                }

                var texture = new Texture2D(device, w, h);
                texture.SetData(colors);
                return texture;
            }
        }

        public static unsafe Texture2D DecodeStream(GraphicsDevice device, FileStream fs) {
            using (var br = new BinaryReader(fs))
            {
                var mn = br.ReadInt32(); // Magic number
                var w = br.ReadInt32();
                var h = br.ReadInt32();
                var pl = br.ReadInt32();

                var paletteBytes = br.ReadBytes(pl*4);
                var indicies = br.ReadBytes(w*h*2);

                var colors = new uint[w*h];
                {
                    fixed (byte* ibp = &indicies[0])
                    fixed (byte* pbp = &paletteBytes[0])
                    {
                        for (int i = 0; i < w*h; i++)
                        {
                            {
                                var isp = (ushort*) (ibp) + i;
                                var index = *isp;

                                var cip = (uint*) (pbp) + index;
                                var color = *cip;

                                colors[i] = color;
                            }
                        }
                    }
                }

                var texture = new Texture2D(device, w, h);
                texture.SetData(colors);
                return texture;
            }
        }

        public static Texture2D ReadFromFileSlow(GraphicsDevice device, string filename) {

             using(var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
             {
                 using(var br = new BinaryReader(fs))
                 {
                     var mn = br.ReadInt32();    // Magic number
                     var w = br.ReadInt32();
                     var h = br.ReadInt32();

                     var pl = br.ReadInt32();
                     
                     var paletteBytes = br.ReadBytes(pl*4);
                     var indicies = br.ReadBytes(w*h*2);

                     var colors = new uint[w*h];

                        for(int i=0;i<w*h;i++)
                        {
                            var bc = i*2;
                            var index = (ushort)(indicies[bc] | indicies[bc+1] << 8);

                            var bp = index*4;
                            var color = (uint) (paletteBytes[bp] | paletteBytes[bp+1] << 8 | paletteBytes[bp+2] << 16 | paletteBytes[bp+3] << 24);
                            colors[i] = color;
                        }
                     
                     var texture = new Texture2D(device, w, h);
                     texture.SetData(colors);
                     return texture;
                 }
             }
        }

        private static Dictionary<uint, ushort> GetPalette(Texture2D image) {
            
            var colorData = new uint[image.Width*image.Height];
            image.GetData(colorData);

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


            var paletteDictionary = new Dictionary<uint, ushort>();
            var pallete = new uint[results.Count];
            {
                var i=0;
                foreach (var keyValuePair in results)
                {
                    pallete[i] = keyValuePair.Key;
                    paletteDictionary.Add(pallete[i], (ushort)i);
                    i++;
                }
            }

            return paletteDictionary;
        }


    }
}
