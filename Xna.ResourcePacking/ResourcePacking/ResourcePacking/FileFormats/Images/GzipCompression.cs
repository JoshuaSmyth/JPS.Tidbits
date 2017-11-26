using System;
using System.IO;
using System.IO.Compression;

namespace Loading_a_3D_model.Compression
{
    class GzipCompression
    {
        public static byte[] Compress(byte[] buffer)
        {
            using(var ms = new MemoryStream())
            {
                using(var zip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zip.Write(buffer, 0, buffer.Length);
                    zip.Close();
                    ms.Position = 0;

                    var compressed = new byte[ms.Length];
                    ms.Read(compressed, 0, compressed.Length);

                    var gzBuffer = new byte[compressed.Length + 4];
                    Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
                    Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
                    return gzBuffer;
                }
            }
        }
        public static byte[] Decompress(byte[] gzBuffer)
        {
            using(var ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                var buffer = new byte[msgLength];

                ms.Position = 0;
                using(var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                    return buffer;
                }
            }
        }
    }
}
