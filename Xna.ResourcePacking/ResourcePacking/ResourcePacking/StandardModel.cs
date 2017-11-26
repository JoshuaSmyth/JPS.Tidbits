using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Loading_a_3D_model
{
    public class StandardModel
    {
        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;


        public Int32 PrimativeCount {
            get
            {
                return IndexBuffer.IndexCount/3;
            }
        }

        public Int32 VertexCount {
            get {
                return VertexBuffer.VertexCount;
            }
        }
    }
}
