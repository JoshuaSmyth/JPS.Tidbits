using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Loading_a_3D_model.Fog
{
    public class Fog
    {
        // World Space remember Y is up in our coordinate space

        public VertexPositionNormalTexture[] Verts = new VertexPositionNormalTexture[4];
        public short[] Indicies = new short[6];

        public Fog() {

        }

        public void Draw() {
            
        }
    }
}
