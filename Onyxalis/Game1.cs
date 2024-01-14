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
        Texture2D dirtTexture;

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
            dirtTexture = Content.Load<Texture2D>("Dirt");
            textureDictionary.Add(Tile.TileType.DIRT, Content.Load<Texture2D>("Dirt"));
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
            world.loadedChunks.Clear();
            for (int x = -5; x < 5; x++)
            {
                for (int y = -5; y < 5; y++)
                {
                    world.LoadChunk((int)(player.position.X / Tile.tilesize / 64 + x), (int)(player.position.Y / Tile.tilesize / 64) + y);
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
                    for (int i = 0; i < world.loadedChunks.Count; i++)
                    {
                        (int x, int y, ChunkCluster cluster) = world.loadedChunks[i];
                        Chunk chunk = cluster.chunks[x, y];
                        (int tX, int tY) = (chunk.x * 16 + cluster.x * 1024, chunk.y * 16 + cluster.y * 1024);
                        for (int X = 0; X < 64; X++)
                        {
                            for (int Y = 0; Y < 64; Y++)
                            {
                                Tile tile = chunk.tiles[X, Y];
                                if (tile != null)
                                {
                                    //textureDictionary.GetValueOrDefault(tile.Type)
                                    if (Keyboard.GetState().IsKeyDown(Keys.F))
                                    {
                                        player.position.X = tile.x * Tile.tilesize;
                                        player.position.Y = tile.y * Tile.tilesize;
                                    }
                                        _spriteBatch.Draw(dirtTexture, new Vector2(tile.x * Tile.tilesize - camera.position.X, tile.y * -Tile.tilesize + camera.position.Y), null, Color.White, 0, new Vector2(), 2, SpriteEffects.None, 0);
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