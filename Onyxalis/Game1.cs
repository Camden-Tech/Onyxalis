using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Onyxalis.Objects.Worlds;
using Onyxalis.Objects.Entities;
using Onyxalis.Objects.UI;
using System;
using System.Collections.Generic;
using MiNET.Blocks;
using System.Diagnostics;

namespace Onyxalis
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public static Random GameRandom = new Random();
        World world;
        Player player = new Player();
        Objects.UI.Camera camera = new Objects.UI.Camera();
        GameState state = GameState.Menu;
        public Dictionary<Tile.TileType, Texture2D> textureDictionary = new Dictionary<Tile.TileType, Texture2D>();


        public enum GameState
        {
            Menu,
            Game
        }



        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        public bool BeginGameCreation()
        {
            try
            {

                world = World.CreateWorld();
                Vector2 spawnLoc = world.GenerateSpawnLocation();
                player.position = spawnLoc;
                Debug.WriteLine("asd ");
                state = GameState.Game;
                 
            }
            catch (Exception e)
            {
                //Log exception, make a popup with the error and a generated file.
                return false;
            }
            return true;
        }



        protected override void Initialize()
        {

            // TODO: Add your initialization logic here
            BeginGameCreation();
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            textureDictionary.Add(Tile.TileType.DIRT, Content.Load<Texture2D>("Dirt"));
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                camera.position.X += 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                camera.position.X -= 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                camera.position.Y += 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                camera.position.X -= 1;
            }
            camera.position = player.position;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Debug.WriteLine("1a ");
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            
            switch (state)
            {
                case GameState.Menu:
                    // Draw lobby screen
                    break;

                case GameState.Game:
                    Debug.WriteLine("2 ");
                    for (int i = 0; i < world.loadedChunks.Count; i++)
                    {
                        Debug.WriteLine("3 ");
                        (int x, int y) = world.loadedChunks[i];
                        Chunk chunk = world.chunks[x, y];
                        for (int X = 0; X < 64; X++)
                        {
                            Debug.WriteLine("4 ");
                            for (int Y = 0; Y < 64; Y++)
                            {
                                Tile tile = chunk.tiles[X, Y];
                                Debug.WriteLine("Tiles "+tile.Type);
                                if (tile != null)
                                {
                                    Debug.WriteLine("5 ");
                                    _spriteBatch.Draw(textureDictionary.GetValueOrDefault(tile.Type),new Vector2(tile.x - camera.position.X,tile.y - camera.position.Y), Color.White);
                                }
                            }
                        }
                        
                    }
                    
                    break;
            }

            _spriteBatch.End();

        

            base.Draw(gameTime);
        }
    }
}