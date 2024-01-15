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
using Lucene.Net.Support;
using Onyxalis.Objects.Math;

namespace Onyxalis
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public static Random GameRandom = new Random();
        public static World world;
        public float time = 0;
        Player player = new Player();
        Objects.UI.Camera camera = new Objects.UI.Camera();
        GameState state = GameState.Menu;
        public Dictionary<Tile.TileType, Texture2D> tileTextureDictionary = new Dictionary<Tile.TileType, Texture2D>();
        public Dictionary<Player.PlayerTextures, Texture2D> playerTextureDictionary = new Dictionary<Player.PlayerTextures, Texture2D>();

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
                //player.position = spawnLoc;
                player.position = new Vector2(500,500);
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
            tileTextureDictionary.Add(Tile.TileType.DIRT1, Content.Load<Texture2D>("Dirt"));
            tileTextureDictionary.Add(Tile.TileType.DIRT2, Content.Load<Texture2D>("DirtTwo"));
            tileTextureDictionary.Add(Tile.TileType.GRASS, Content.Load<Texture2D>("Grass"));
            tileTextureDictionary.Add(Tile.TileType.DIRT3, Content.Load<Texture2D>("DirtThree"));
            tileTextureDictionary.Add(Tile.TileType.GRASS2, Content.Load<Texture2D>("GrassTwo"));
            playerTextureDictionary.Add(Player.PlayerTextures.Body, Content.Load<Texture2D>("BeautifulPlayerCharacter"));
            tileTextureDictionary.Add(Tile.TileType.DIRT4, Content.Load<Texture2D>("DirtFour"));
            // TODO: use this.Content to load your game content here
            Texture2D texture = playerTextureDictionary[Player.PlayerTextures.Body];
            player.hitbox = new Hitbox(new Vector2[] { new Vector2(0,0), new Vector2(texture.Width, 0), new Vector2(texture.Width, texture.Height), new Vector2(0, texture.Height)}, player.position);
        }

        protected override void Update(GameTime gameTime)
        {
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            camera.position = player.position - new Vector2(1920 / 2, -1080 / 2);
            player.Acceleration = Vector2.Zero;
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                player.Acceleration.Y = 15;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                player.Acceleration.Y = -15;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                player.Acceleration.X = 15;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                player.Acceleration.X = -15;
            }
            
            for (int x = -5; x < 5; x++)
            {
                for (int y = -5; y < 5; y++)
                {
                    if ((int)(player.position.X / (Tile.tilesize) / 64 + x) < -500 || (int)(player.position.Y / (Tile.tilesize) / 64) < -500)
                    {
                        //asda
                    }
                    Chunk c = world.LoadChunk((int)(player.position.X / (Tile.tilesize) / 64 + x), (int)(player.position.Y / (Tile.tilesize) / 64) + y);

                    c.loaded = true;
                }
            }

            List<Hitbox> tileHitboxes = new List<Hitbox>();
            (int X, int Y) = World.findTilePosition(player.position.X, player.position.Y);
            Tile tile = world.tiles[X, Y];
            if (tile != null)
            {
                tileHitboxes.Add(tile.hitbox);
            }
            tile = new Tile();
            tile.x = X;
            tile.y = Y;
            tile.Type = Tile.TileType.GRASS;
            world.tiles[X, Y] = tile;
            
            
            player.Process_((float)gameTime.ElapsedGameTime.TotalSeconds, tileHitboxes.ToArray());
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

        public void drawTiles()
        {
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
                                Texture2D tileTexture = tileTextureDictionary.GetValueOrDefault(tile.Type);
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
        }
        public void drawPlayer()
        {

            Texture2D texture = playerTextureDictionary[Player.PlayerTextures.Body];
            _spriteBatch.Draw(texture, new Vector2(1920 - texture.Width / 2, 1080 - texture.Height / 2) / 2, null, Color.White, 0, new Vector2(), 2, SpriteEffects.None, 0);
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
                    drawTiles();
                    drawPlayer();
                    
                    break;
            }

            _spriteBatch.End();

        

            base.Draw(gameTime);
        }
    }
}