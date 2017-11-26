using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

// TODO: replace these with the processor input and output types.
using Microsoft.Xna.Framework.Graphics.PackedVector;
using TInput = System.String;
using TOutput = System.String;

namespace TextureExtension
{
    [ContentProcessor(DisplayName = "TextureExtension.alpha8")]
    public class TexAlpha8 : ContentProcessor<Texture2DContent, Texture2DContent>
    {
        public override Texture2DContent Process(Texture2DContent input, ContentProcessorContext context)
        {
            // Note: In the shader the color is in the w component after sampling.

            input.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>));
             MipmapChain mipmapChain = input.Faces[0];
            foreach (PixelBitmapContent<Vector4> bitmap in mipmapChain)
            {

                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        Vector4 pixel = bitmap.GetPixel(x, y);
                        bitmap.SetPixel(x, y, new Vector4(0, 0, 0, pixel.X));
                    }
                }
            }

            input.ConvertBitmapType(typeof(PixelBitmapContent<Alpha8>));
            input.GenerateMipmaps(false);

            return input;
        }
    }
}