using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Loading_a_3D_model.Compression;
using Loading_a_3D_model.FileFormats.Images;
using Loading_a_3D_model.FileFormats.Meshes;
using Loading_a_3D_model.Pack;
using Loading_a_3D_model.Scheduler;
using Maglev.Monorail.Scheduler;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;


namespace Loading_a_3D_model
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Model pipe;
        private Model rocks;

        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 3, -12), new Vector3(0, 3, 0), Vector3.UnitY);
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 1280 / 720.0f, 0.1f, 100f);

        private Effect Standard;
        private Effect Half;

        private Texture WallTexture;
        private Texture RockTexture;
        private Texture DoorBarsTexture;
        private Texture FloorTexture;
        private Texture GoatTexture;
        private Texture IceTexture;
        private Texture TorchTexture;
        private Texture WaterTexture;

        private float z = -12;

        StandardModel NewModelPipe;
        StandardModel NewModelRocks;

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
        /// LoadContent will be called once per game and is the place to load\\
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            GraphicsDevice.PresentationParameters.MultiSampleCount = 8;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            Standard = Content.Load<Effect>("Standard");
            Half = Content.Load<Effect>("Half");

             Console.WriteLine("LOADING TEXTURES");
           
            var sw = new Stopwatch();

            {
                sw.Start();
                    NewModelRocks = ModelLoader.LoadStandardModel(GraphicsDevice, "Assets/Models/rocks.smod");
                    NewModelPipe = ModelLoader.LoadStandardModel(GraphicsDevice, "Assets/Models/pipe.smod");
                sw.Stop();
                Console.WriteLine("Time taken to load model via file:" + sw.ElapsedMilliseconds + "ms");
                sw.Reset();
            }

            {
                // This way appears slightly faster (~10%) but I don't think it's worth the trade off of loading the entire chunk into memory
                var jobScheduler = new JobScheduler();
                var chunkReader = new AssetChunkReader();
                
                var chunkFileName = "Assets/Chunk/textures.chunk";

                chunkReader.AssociateChunk(chunkFileName);
                
                sw.Start();
                
                var fs1 = new FileStream(chunkFileName,FileMode.Open,FileAccess.Read,FileShare.Read);
                var fs2 = new FileStream(chunkFileName,FileMode.Open,FileAccess.Read,FileShare.Read);
                var fs3 = new FileStream(chunkFileName,FileMode.Open,FileAccess.Read,FileShare.Read);
                var fs4 = new FileStream(chunkFileName,FileMode.Open,FileAccess.Read,FileShare.Read);
                var fs5 = new FileStream(chunkFileName,FileMode.Open,FileAccess.Read,FileShare.Read);
                var fs6 = new FileStream(chunkFileName,FileMode.Open,FileAccess.Read,FileShare.Read);
                var fs7 = new FileStream(chunkFileName,FileMode.Open,FileAccess.Read,FileShare.Read);
                var fs8 = new FileStream(chunkFileName,FileMode.Open,FileAccess.Read,FileShare.Read);


                jobScheduler.QueueJob("Assets/Textures/tex16/walls.tex16",() => { WallTexture = chunkReader.LoadTextureFromStream(GraphicsDevice, "Assets/Textures/tex16/walls.tex16", fs1); });
                jobScheduler.QueueJob("Assets/Textures/tex16/rocks.tex16",() => { RockTexture = chunkReader.LoadTextureFromStream(GraphicsDevice, "Assets/Textures/tex16/rocks.tex16", fs2); });
                
                jobScheduler.QueueJob("Assets/Textures/tex16/doorbars.tex16",() => { DoorBarsTexture = chunkReader.LoadTextureFromStream(GraphicsDevice, "Assets/Textures/tex16/doorbars.tex16", fs3); });
                jobScheduler.QueueJob("Assets/Textures/tex16/floor.tex16",() => { FloorTexture = chunkReader.LoadTextureFromStream(GraphicsDevice, "Assets/Textures/tex16/floor.tex16", fs4); });
            
                jobScheduler.QueueJob("Assets/Textures/tex16/goat.tex16",() => { GoatTexture = chunkReader.LoadTextureFromStream(GraphicsDevice, "Assets/Textures/tex16/goat.tex16", fs5); });
                jobScheduler.QueueJob("Assets/Textures/tex16/ice.tex16",() => { IceTexture = chunkReader.LoadTextureFromStream(GraphicsDevice, "Assets/Textures/tex16/ice.tex16", fs6); });
                jobScheduler.QueueJob("Assets/Textures/tex16/torch.tex16",() => { TorchTexture = chunkReader.LoadTextureFromStream(GraphicsDevice, "Assets/Textures/tex16/torch.tex16", fs7); });
                jobScheduler.QueueJob("Assets/Textures/tex16/water.tex16",() => { WaterTexture = chunkReader.LoadTextureFromStream(GraphicsDevice, "Assets/Textures/tex16/water.tex16", fs8); });
                
                jobScheduler.ExecuteAll(() =>
                {
                    fs1.Close();
                    fs2.Close();
                    fs3.Close();
                    fs4.Close();
                    fs5.Close();
                    fs6.Close();
                    fs7.Close();
                    fs8.Close();

                    sw.Stop();
                    Console.WriteLine("Time taken to load tex16 (multithreaded) from Chunk file: " + sw.ElapsedMilliseconds + "ms");
                    sw.Reset();
                });
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

        
        private void DrawModelHalf(StandardModel model, Texture texture, Matrix world, Matrix view, Matrix projection)
        {
                var worldParam = Half.Parameters["World"];
                var viewParam = Half.Parameters["View"];
                var projParam = Half.Parameters["Projection"];
                var textParam = Half.Parameters["gTex0"];
                var camPos = Half.Parameters["CameraPosition"];
                camPos.SetValue(new Vector3(0, 3, z));
                textParam.SetValue(texture);
            
                graphics.GraphicsDevice.SetVertexBuffer(model.VertexBuffer, 0);
                graphics.GraphicsDevice.Indices = model.IndexBuffer;

                worldParam.SetValue(world);
                viewParam.SetValue(view);
                projParam.SetValue(projection);
                Half.CurrentTechnique.Passes[0].Apply();
                graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, model.VertexCount, 0, model.PrimativeCount);
            
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            graphics.GraphicsDevice.Clear(Color.Black);
            view = Matrix.CreateLookAt(new Vector3(0, 3, z), new Vector3(0, 3, z+12), Vector3.UnitY);
            
            //DrawModel(pipe, WallTexture, Matrix.CreateTranslation(new Vector3(0, 0, 36f)), view, projection);
            //DrawModel(rocks, RockTexture, Matrix.CreateTranslation(new Vector3(0, 0, 36f)), view, projection);
            
            DrawModelHalf(NewModelPipe, WallTexture, Matrix.CreateTranslation(new Vector3(0, 0, 24f)), view, projection);
            DrawModelHalf(NewModelRocks, RockTexture, Matrix.CreateTranslation(new Vector3(0, 0, 24f)), view, projection);
            
            DrawModelHalf(NewModelPipe, WallTexture, Matrix.CreateTranslation(new Vector3(0, 0, 12f)), view, projection);
            DrawModelHalf(NewModelRocks, RockTexture,Matrix.CreateTranslation(new Vector3(0, 0, 12f)), view, projection);
            
            DrawModelHalf(NewModelPipe, WallTexture, Matrix.CreateTranslation(new Vector3(0, 0, 0f)), view, projection);
            DrawModelHalf(NewModelRocks,RockTexture, Matrix.CreateTranslation(new Vector3(0, 0, 0f)), view, projection);
            
            DrawModelHalf(NewModelPipe, WallTexture,Matrix.CreateTranslation(new Vector3(0, 0, -12f)), view, projection);
            DrawModelHalf(NewModelRocks, RockTexture,Matrix.CreateTranslation(new Vector3(0, 0, -12f)), view, projection);
            
            DrawModelHalf(NewModelPipe, WallTexture, Matrix.CreateTranslation(new Vector3(0, 0, -24f)), view, projection);
            DrawModelHalf(NewModelRocks, RockTexture,Matrix.CreateTranslation(new Vector3(0, 0, -24f)), view, projection);

            DrawModelHalf(NewModelPipe, WallTexture, Matrix.CreateTranslation(new Vector3(0, 0, -36f)), view, projection);
            DrawModelHalf(NewModelRocks,RockTexture, Matrix.CreateTranslation(new Vector3(0, 0, -36f)), view, projection);
            
            DrawModelHalf(NewModelPipe, WallTexture, Matrix.CreateTranslation(new Vector3(0, 0, -48f)), view, projection);
            DrawModelHalf(NewModelRocks, RockTexture,Matrix.CreateTranslation(new Vector3(0, 0, -48f)), view, projection);

            DrawModelHalf(NewModelPipe,WallTexture, Matrix.CreateTranslation(new Vector3(0, 0, -60f)), view, projection);
            DrawModelHalf(NewModelRocks,RockTexture, Matrix.CreateTranslation(new Vector3(0, 0, -60f)), view, projection);
        }
    }
}
