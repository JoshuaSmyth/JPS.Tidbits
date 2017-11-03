using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Loading_a_3D_model
{
    public struct VertexPositionNormalTextureColor
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 texcoord;
        public Color color;

        public VertexPositionNormalTextureColor(Vector3 position, Vector3 normal, Vector2 texcoord, Color color)
        {
            this.position = position;
            this.normal = normal;
            this.texcoord = texcoord;
            this.color = color;
        }

        public static VertexElement[] VertexElements =
        {
             new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
             new VertexElement(3*sizeof(float), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
             new VertexElement(6*sizeof(float), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
             new VertexElement(8*sizeof(float), VertexElementFormat.Color, VertexElementUsage.Color, 0)
        };

        public static int SizeInBytes = 9 * sizeof(float);
    }
}
