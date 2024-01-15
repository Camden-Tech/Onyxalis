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
using Onyxalis.Objects.Tiles;

namespace Onyxalis
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public static Random GameRandom = new Random();
        public static World world;
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
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();
            
        }

        public bool BeginGameCreation()
        {
            try
            {

                world = World.CreateWorld();
                Vector2 spawnLoc = world.GenerateSpawnLocation();
                spawnLoc.Y *= -1;
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
            textureDictionary.Add(Tile.TileType.DIRT1, Content.Load<Texture2D>("Dirt"));
            textureDictionary.Add(Tile.TileType.DIRT2, Content.Load<Texture2D>("DirtTwo"));
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            camera.position = player.position;
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                player.position.Y += 64;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                player.position.Y -= 64;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                player.position.X += 64;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                player.position.X -= 64;
            }
            
            for (int x = -5; x < 5; x++)
            {
                for (int y = -5; y < 5; y++)
                {
                    Chunk c = world.LoadChunk((int)(player.position.X / Tile.tilesize / 64 + x), (int)(player.position.Y / Tile.tilesize / 64) + y);
                    c.loaded = true;
                }
            }
            foreach ((int x, int y) pos in world.loadedChunks.Keys)
            {
                Chunk c = world.loadedChunks[pos];
                if (!c.loaded)
                {
                    world.UnloadChunk(pos);
                } else
                {
                    c.loaded = false;
                    world.loadedChunks[pos] = c;
                }


            }
            
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                   SamplerState.PointClamp, DepthStencilState.Default,
                   RasterizerState.CullCounterClockwise, null,
                   Matrix.Identity);

            switch (state)
            {
                case GameState.Menu:
                    // Draw lobby screen
                    break;

                case GameState.Game:
                    foreach (Chunk chunk in world.loadedChunks.Values)
                    {
                        for (int X = 0; X < 64; X++)
                        {
                            for (int Y = 0; Y < 64; Y++)
                            {
                                Tile tile = chunk.tiles[X, Y];
                                if (tile != null)
                                {
                                    Vector2 pos = new Vector2(tile.x * Tile.tilesize - camera.position.X, tile.y * -Tile.tilesize + camera.position.Y);
                                    if (pos.X > -Tile.tilesize && pos.X < 1980 + Tile.tilesize && pos.Y > -Tile.tilesize && pos.Y < 1080 + Tile.tilesize)
                                    {
                                        Texture2D tileTexture = textureDictionary.GetValueOrDefault(tile.Type);
                                        Vector2 origin = new Vector2(tileTexture.Width / 2f, tileTexture.Height / 2f);

                                        _spriteBatch.Draw(tileTexture, pos, null, Color.White, MathHelper.ToRadians(90 * tile.rotation), origin, 2, SpriteEffects.None, 0);

                                        
                                    }
                                    if (Keyboard.GetState().IsKeyDown(Keys.F))
                                    {
                                        player.position.X = tile.x * Tile.tilesize;
                                        player.position.Y = tile.y * Tile.tilesize;
                                    }
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