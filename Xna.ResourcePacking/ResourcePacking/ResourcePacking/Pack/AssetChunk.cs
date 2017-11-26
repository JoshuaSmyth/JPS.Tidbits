using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Loading_a_3D_model.FileFormats.Images;
using Microsoft.Xna.Framework.Graphics;

namespace Loading_a_3D_model.Pack
{
    class AssetRecordHeader
    {
        public byte[] Name;
        public long Length;
        public long Offset;     // From begining of the file
        
        public string GetName() {
            return Encoding.ASCII.GetString(Name);
        }

        public void SetName(string value) {
            Name = Encoding.ASCII.GetBytes(value);
        }

        public void SetName(byte[] value) {
            Name = value;
        }
    }

    public class AssetChunkBuilder
    {
        readonly List<AssetRecordHeader> m_AssetHeaders = new List<AssetRecordHeader>();

        readonly List<byte[]> m_AssetData = new List<byte[]>();

        public void AddAsset(String name, byte[] data) 
        {
            // TODO Don't add duplicates

            var record = new AssetRecordHeader();
            record.Length = data.Length;

            record.SetName(name);

            m_AssetHeaders.Add(record);
            m_AssetData.Add(data);
        }

        public void WriteChunkToFile(string filename) 
        {
            // Calculate the header size
            long headerSize = 0;
            headerSize+=sizeof(int);    // Asset Count;
            for(int i=0;i<m_AssetHeaders.Count;i++)
            {
                headerSize+=sizeof(int);    // Name Length
                headerSize+=m_AssetHeaders[i].Name.Length;
                headerSize+=sizeof(long);
                headerSize+=sizeof(long);
            }
            
            // Update the records
            long assetOffset = headerSize;
            for (int i = 0; i < m_AssetHeaders.Count; i++)
            {
                m_AssetHeaders[i].Offset = assetOffset;
                assetOffset+=m_AssetData[i].Length;
            }

            using(var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using(var bw = new BinaryWriter(fs))
                {
                    bw.Write((Int32) m_AssetData.Count);

                    for(int i=0;i<m_AssetHeaders.Count;i++)
                    {
                        bw.Write((Int32)m_AssetHeaders[i].Name.Length);
                        bw.Write(m_AssetHeaders[i].Name);
                        bw.Write((long)m_AssetHeaders[i].Length);
                        bw.Write((long)m_AssetHeaders[i].Offset);
                    }

                    for(int i=0;i<m_AssetData.Count;i++)
                    {
                        bw.Write(m_AssetData[i]);
                    }
                }
            }
        }
    }

    public class AssetChunkReader
    {
        String m_FileName;

        readonly Dictionary<String, AssetRecordHeader> m_Headers = new Dictionary<string, AssetRecordHeader>();

        // FileStream per thread?

        public void AssociateChunk(string filename) {
            m_FileName = filename;

            using(var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                AssiociateChuckFromStream(fs);
            }
        }

        private void AssiociateChuckFromStream(Stream fs) {
            using (var br = new BinaryReader(fs))
            {
                var count = br.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    var nameLength = br.ReadInt32();
                    var name = br.ReadBytes(nameLength);
                    var fileLength = br.ReadInt64();
                    var offset = br.ReadInt64();

                    var record = new AssetRecordHeader();
                    record.SetName(name);
                    record.Length = fileLength;
                    record.Offset = offset;

                    m_Headers.Add(record.GetName(), record);
                }
            }
        }

        public void AssociateChunk(byte[] bytes) {
            using(var ms = new MemoryStream(bytes))
            {
                AssiociateChuckFromStream(ms);
            }
        }

        public byte[] LoadData(string name) {
            
            var record = m_Headers[name];
            var offset = record.Offset;   // ?
            using(var fs = new FileStream(m_FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fs.Seek(offset, SeekOrigin.Begin);
                using(var br = new BinaryReader(fs))
                {
                    return br.ReadBytes((Int32)record.Length);
                }
            }
        }

        //public Texture LoadTextureFromFile(GraphicsDevice device, string name) {
        //    var record = m_Headers[name];
        //    var offset = record.Offset;
        //    using(var fs = new FileStream(m_FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        //    {
        //        fs.Seek(offset, SeekOrigin.Begin);
        //        return Tex16.DecodeStream(device, fs);
        //    }
        //}

        public Texture LoadTextureFromStream(GraphicsDevice device, string name, FileStream fs) {
            var record = m_Headers[name];
            var offset = record.Offset;
            //using(var fs = new FileStream(m_FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fs.Seek((int)offset, SeekOrigin.Begin);
                return Tex16.DecodeStream(device, fs);
            }
        }

        public Texture LoadTextureFromStream(GraphicsDevice device, string name, byte[] chunkFile) {
            var record = m_Headers[name];
            var offset = record.Offset;
            //using(var fs = new MemoryStream(chunkFile))
            {
              //  fs.Seek((int)offset, SeekOrigin.Begin);
                return Tex16.DecodeFromBytes(device, chunkFile, (int)offset);
            }
        }
    }
}
