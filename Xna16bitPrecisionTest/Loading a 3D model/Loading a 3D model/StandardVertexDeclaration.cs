using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Loading_a_3D_model
{
        struct StandardVertexDeclaration : IVertexType
    {
        public HalfVector4 PositionPlusUVx;
        public HalfVector4 NormalPlusUVy;

        public readonly static VertexDeclaration Declaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.HalfVector4, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.HalfVector4, VertexElementUsage.Normal, 0)
        );

        public VertexDeclaration VertexDeclaration { get { return StandardVertexDeclaration.Declaration; } }

        public static Int32 Size { get { return 16; } }
    }

    //struct StandardVertexDeclaration : IVertexType
    //{
    //    public HalfVector4 Normal;
    //    public HalfVector2 Texture;
    //    public Vector3 Position;

    //    // Note the order

    //    public readonly static VertexDeclaration Declaration = new VertexDeclaration
    //    (
    //        new VertexElement(0, VertexElementFormat.HalfVector4, VertexElementUsage.Normal, 0),
    //        new VertexElement(8, VertexElementFormat.HalfVector2, VertexElementUsage.TextureCoordinate, 0),
    //        new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
    //    );

    //    public VertexDeclaration VertexDeclaration { get { return StandardVertexDeclaration.Declaration; } }

    //    public static Int32 Size { get { return 24; } }
    //}
}
