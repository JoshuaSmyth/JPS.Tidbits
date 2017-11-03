using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Loading_a_3D_model
{


    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Model model;
        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 3, -12), new Vector3(0, 3, 0), Vector3.UnitY);
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 1280 / 720.0f, 0.1f, 100f);

       // private Effect Triplanar;
       // private Effect TriplanarBlend;
        private Effect Standard;
         private Effect Half;
        private Texture texture;

        private float z = -12;


        StandardModel NewModel;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferMultiSampling = true;

            this.IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
                GraphicsDevice.PresentationParameters.MultiSampleCount = 8;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            Standard = Content.Load<Effect>("Standard");
            Half = Content.Load<Effect>("Half");

            texture = Content.Load<Texture>("WallDIF");
              var sw = new Stopwatch();
            sw.Start();
                model = Content.Load<Model>("Cave_Tunnel_Pipe");
                Console.WriteLine("Time taken to load model via contentpipeline:" + sw.ElapsedMilliseconds + "ms");
            sw.Stop();
          
            sw.Reset();
            sw.Start();
                using(var fs = new FileStream("temp4.smod",FileMode.Open,FileAccess.Read))
                {
                    using(var br=new BinaryReader(fs))
                    {
                        var id = br.ReadInt32();
                        var indexLenth = br.ReadInt32();
                        var vertLength = br.ReadInt32();

                        var indexArray = br.ReadBytes(indexLenth);
                        var byteArray = br.ReadBytes(vertLength);

                        NewModel = new StandardModel();
                        NewModel.VertexBuffer = new VertexBuffer(GraphicsDevice, StandardVertexDeclaration.Declaration, byteArray.Length, BufferUsage.None);
                        NewModel.VertexBuffer.SetData(byteArray);

                        NewModel.IndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indexArray.Length/2, BufferUsage.None);
                        NewModel.IndexBuffer.SetData(indexArray);
                    }
                }
            sw.Stop();
            Console.WriteLine("Time taken to load model via file:" + sw.ElapsedMilliseconds + "ms");

        //    SaveModelFile();



            // TODO: use this.Content to load your game content here
        }

        private void SaveModelFile() {
// Save model
            var vb = model.Meshes[0].MeshParts[0].VertexBuffer;

            //  var verts = vb.GetData()

            var elements = vb.VertexDeclaration.GetVertexElements();
            var data = new VertexPositionNormalTextureColor[vb.VertexCount];
            vb.GetData(data);

            var ib = model.Meshes[0].MeshParts[0].IndexBuffer;
            var indexByteArray = new byte[2*ib.IndexCount];
            ib.GetData(indexByteArray);

            // 16 bit index buffer + 16 byte vertex buffer;
            var standardV = new StandardVertexDeclaration[vb.VertexCount];
            for (int i = 0; i < vb.VertexCount; i++)
            {
                standardV[i].PositionPlusUVx = new HalfVector4(data[i].position.X, data[i].position.Y, data[i].position.Z,data[i].texcoord.X);
                standardV[i].NormalPlusUVy = new HalfVector4(data[i].normal.X, data[i].normal.Y, data[i].normal.Z,data[i].texcoord.Y);
            }

                 var byteArray = ByteMarshal.WriteDataMarshalCopy(standardV);

           // vb.SetData(standardV);
            NewModel = new StandardModel();
            NewModel.VertexBuffer = new VertexBuffer(GraphicsDevice, StandardVertexDeclaration.Declaration, standardV.Length, BufferUsage.None);
            NewModel.VertexBuffer.SetData(standardV);

            NewModel.IndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, ib.IndexCount, BufferUsage.None);
            NewModel.IndexBuffer.SetData(indexByteArray);


           
       

          //   Write new file
            using (var fs = new FileStream("temp4.smod", FileMode.CreateNew))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write((Int32) 42);
                    bw.Write((Int32) indexByteArray.Length);
                    bw.Write((Int32) byteArray.Length);
                    bw.Write(indexByteArray);
                    bw.Write(byteArray);
                }
            }
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            var keystate = Keyboard.GetState();
            if (keystate.IsKeyDown(Keys.A))
            {
                z+=0.1f;
            }
            if (keystate.IsKeyDown(Keys.Z))
            {
                z-=0.1f;
            }

        }

        
        private void DrawModelHalf(Model model, Matrix world, Matrix view, Matrix projection)
        {
                var worldParam = Half.Parameters["World"];
                var viewParam = Half.Parameters["View"];
                var projParam = Half.Parameters["Projection"];
                var textParam = Half.Parameters["gTex0"];
                var camPos = Half.Parameters["CameraPosition"];
                camPos.SetValue(new Vector3(0, 3, z));
                textParam.SetValue(texture);
            
             //   foreach (ModelMesh mesh in model.Meshes)
                {
               //     foreach (var part in mesh.MeshParts)
                    {
                        graphics.GraphicsDevice.SetVertexBuffer(NewModel.VertexBuffer, 0);
                        graphics.GraphicsDevice.Indices = NewModel.IndexBuffer;
                        worldParam.SetValue(world);
                        viewParam.SetValue(view);
                        projParam.SetValue(projection);
                        Half.CurrentTechnique.Passes[0].Apply();
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, NewModel.VertexCount, 0, NewModel.PrimativeCount);

//                        graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, NewModel.PrimativeCount);

                    }
                }
        }

        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
                var worldParam = Standard.Parameters["World"];
                var viewParam = Standard.Parameters["View"];
                var projParam = Standard.Parameters["Projection"];
                var textParam = Standard.Parameters["gTex0"];
                var camPos = Standard.Parameters["CameraPosition"];
                camPos.SetValue(new Vector3(0, 3, z));
                textParam.SetValue(texture);
            
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        graphics.GraphicsDevice.SetVertexBuffer(part.VertexBuffer, part.VertexOffset);
                        graphics.GraphicsDevice.Indices = part.IndexBuffer;
                        worldParam.SetValue(world);
                        viewParam.SetValue(view);
                        projParam.SetValue(projection);
                        Standard.CurrentTechnique.Passes[0].Apply();
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    }
                }
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            graphics.GraphicsDevice.Clear(Color.Black);
            view = Matrix.CreateLookAt(new Vector3(0, 3, z), new Vector3(0, 3, z+12), Vector3.UnitY);
            
            DrawModel(model, Matrix.CreateTranslation(new Vector3(0, 0, 36f)), view, projection);
            DrawModelHalf(model, Matrix.CreateTranslation(new Vector3(0, 0, 24f)), view, projection);
            DrawModelHalf(model, Matrix.CreateTranslation(new Vector3(0, 0, 12f)), view, projection);
            DrawModelHalf(model, Matrix.CreateTranslation(new Vector3(0, 0, 0f)), view, projection);
            DrawModelHalf(model, Matrix.CreateTranslation(new Vector3(0, 0, -12f)), view, projection);
            DrawModelHalf(model, Matrix.CreateTranslation(new Vector3(0, 0, -24f)), view, projection);
            DrawModelHalf(model, Matrix.CreateTranslation(new Vector3(0, 0, -36f)), view, projection);
            DrawModelHalf(model, Matrix.CreateTranslation(new Vector3(0, 0, -48f)), view, projection);
            DrawModelHalf(model, Matrix.CreateTranslation(new Vector3(0, 0, -60f)), view, projection);
        }
    }
}
