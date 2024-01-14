using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Onyxalis.Objects.Worlds;
using Onyxalis.Objects.Entities;
using Onyxalis.Objects.UI;
using System;
using System.Collections.Generic;
using MiNET.Blocks;

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
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            camera.position = player.position;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            
            switch (state)
            {
                case GameState.Menu:
                    // Draw lobby screen
                    break;

                case GameState.Game:
                    for (int i = 0; i < world.loadedChunks.Count; i++)
                    {
                        (int x, int y) = world.loadedChunks[i];
                        Chunk chunk = world.chunks[x, y];
                        for (int X = 0; X < 64; X++)
                        {
                            for (int Y = 0; Y < 64; Y++)
                            {
                                Tile tile = chunk.tiles[X, Y];
                                if (tile != null)
                                {
                                    _spriteBatch.Draw(textureDictionary.GetValueOrDefault(tile.Type),new Vector2(tile.x,tile.y), Color.White);
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