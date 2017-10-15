using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AssetHotloading
{
    public class TextureReference
    {
        public String Id;
        public Texture2D Texture;
    }

    public class HotLoader
    {
        static readonly Dictionary<string, TextureReference> TextureDictionary = new Dictionary<string, TextureReference>(); 

        static FileSystemWatcher _fileSystemWatcher;

        GraphicsDevice m_GraphicsDevice;

        public HotLoader(GraphicsDevice graphicsDevice) {
            m_GraphicsDevice = graphicsDevice;
            Run();
        }

        public void Run() {

            const string fullpath = "Content";
            _fileSystemWatcher = new FileSystemWatcher
            {
                Path = fullpath,
                Filter = "*.*",
                IncludeSubdirectories = true
            };

            // Add event handlers.
            _fileSystemWatcher.Changed += OnChanged;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e) {

            var key = e.FullPath.Replace("\\","/");
            if (TextureDictionary.ContainsKey(key)) {
                var value = TextureDictionary[key];

                Texture2D newTexture;
                using(var fileStream = new FileStream(key, FileMode.Open)) {
                    newTexture = Texture2D.FromStream(m_GraphicsDevice, fileStream);
                }

                value.Texture.Dispose();
                value.Texture = null;
                value.Texture = newTexture;
            }

            Console.WriteLine("File: " +  e.FullPath + " " + e.ChangeType);
        }

        public TextureReference LoadTextureFromStream(string filename) {

            if (TextureDictionary.ContainsKey(filename)) {
                return TextureDictionary[filename];
            }

            var platformTexture = new TextureReference()
            {
                Id = filename
            };
            using(var fileStream = new FileStream(filename, FileMode.Open)) {
                platformTexture.Texture = Texture2D.FromStream(m_GraphicsDevice, fileStream);
            }

            TextureDictionary.Add(filename, platformTexture);
            return platformTexture;
        }

        public void Init(params object[] objects) {
            
        }

        public Dictionary<string, TextureReference> GetTextureDictionary() {
            return TextureDictionary;
        }
    }
}
