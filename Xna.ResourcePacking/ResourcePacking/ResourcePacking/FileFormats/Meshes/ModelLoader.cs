using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Loading_a_3D_model.FileFormats.Meshes
{
    public class ModelLoader
    {
        
        public static StandardModel LoadStandardModel(GraphicsDevice graphicsDevice, string filename) {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    var id = br.ReadInt32();
                    var indexLenth = br.ReadInt32();
                    var vertLength = br.ReadInt32();

                    var indexArray = br.ReadBytes(indexLenth);
                    var byteArray = br.ReadBytes(vertLength);

                    var rv = new StandardModel();
                    rv.VertexBuffer = new VertexBuffer(graphicsDevice, StandardVertexDeclaration.VertexElements, byteArray.Length, BufferUsage.None);
                    rv.VertexBuffer.SetData(byteArray);

                    rv.IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indexArray.Length/2, BufferUsage.None);
                    rv.IndexBuffer.SetData(indexArray);
                    return rv;
                }
            }
        }

    }
}
