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
        public HashMap<Tile.Covering, Dictionary<Tile.TileType, Texture2D>> tileTextureDictionary = new HashMap<int, Dictionary<Tile.TileType, Texture2D>>();
        public HashMap<Tile.Covering, Texture2D> tileOverlays = new HashMap<Tile.Covering, Texture2D>();
        
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
                player.position = spawnLoc;
                player.world = world;
                player.scale = 2;
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
            Dictionary<Tile.TileType, Texture2D> dictionaryDefault = new Dictionary<Tile.TileType, Texture2D>();
            dictionaryDefault.Add(Tile.TileType.DIRT1, Content.Load<Texture2D>("Tiles/Dirt"));
            dictionaryDefault.Add(Tile.TileType.DIRT2, Content.Load<Texture2D>("Tiles/DirtTwo"));
            dictionaryDefault.Add(Tile.TileType.GRASS, Content.Load<Texture2D>("Tiles/Grass"));
            dictionaryDefault.Add(Tile.TileType.DIRT3, Content.Load<Texture2D>("Tiles/DirtThree"));
            dictionaryDefault.Add(Tile.TileType.GRASS2, Content.Load<Texture2D>("Tiles/GrassTwo"));
            dictionaryDefault.Add(Tile.TileType.STONE, Content.Load<Texture2D>("Tiles/Stone"));
            dictionaryDefault.Add(Tile.TileType.DEEPROCK1, Content.Load<Texture2D>("Tiles/DeeprockOne"));
            dictionaryDefault.Add(Tile.TileType.DEEPROCK2, Content.Load<Texture2D>("Tiles/DeeprockTwo"));
            dictionaryDefault.Add(Tile.TileType.DEEPROCK3, Content.Load<Texture2D>("Tiles/DeeprockThree"));
            dictionaryDefault.Add(Tile.TileType.DEEPROCK4, Content.Load<Texture2D>("Tiles/DeeprockFour"));
            dictionaryDefault.Add(Tile.TileType.PERMAFROST1, Content.Load<Texture2D>("Tiles/PermafrostOne"));
            dictionaryDefault.Add(Tile.TileType.PERMAFROST2, Content.Load<Texture2D>("Tiles/PermafrostTwo"));
            dictionaryDefault.Add(Tile.TileType.PERMAFROST3, Content.Load<Texture2D>("Tiles/PermafrostThree"));
            dictionaryDefault.Add(Tile.TileType.PERMAFROST4, Content.Load<Texture2D>("Tiles/PermafrostFour"));
            dictionaryDefault.Add(Tile.TileType.SHORTGRASS, Content.Load<Texture2D>("Tiles/Shortgrass"));
            dictionaryDefault.Add(Tile.TileType.TALLGRASS, Content.Load<Texture2D>("Tiles/Tallgrass"));
            dictionaryDefault.Add(Tile.TileType.COPPERDEEPROCK, Content.Load<Texture2D>("Tiles/CopperDeeprock"));
            dictionaryDefault.Add(Tile.TileType.SNOW1, Content.Load<Texture2D>("Tiles/SnowOne"));
            dictionaryDefault.Add(Tile.TileType.SNOW2, Content.Load<Texture2D>("Tiles/SnowTwo"));
            dictionaryDefault.Add(Tile.TileType.SNOW3, Content.Load<Texture2D>("Tiles/SnowThree"));
            dictionaryDefault.Add(Tile.TileType.SNOW4, Content.Load<Texture2D>("Tiles/SnowFour"));
            dictionaryDefault.Add(Tile.TileType.SAND1, Content.Load<Texture2D>("Tiles/SandOne"));
            dictionaryDefault.Add(Tile.TileType.SAND2, Content.Load<Texture2D>("Tiles/SandTwo"));
            dictionaryDefault.Add(Tile.TileType.SAND3, Content.Load<Texture2D>("Tiles/SandThree"));
            dictionaryDefault.Add(Tile.TileType.SAND4, Content.Load<Texture2D>("Tiles/SandFour"));
            dictionaryDefault.Add(Tile.TileType.SHRUB, Content.Load<Texture2D>("Tiles/Shrub"));
            playerTextureDictionary.Add(Player.PlayerTextures.Body, Content.Load<Texture2D>("Player/BeautifulPlayerCharacter"));
            dictionaryDefault.Add(Tile.TileType.DIRT4, Content.Load<Texture2D>("Tiles/DirtFour"));
            
            tileOverlays[Tile.Covering.Moss] = Content.Load<Texture2D>("Overlays/Moss");
            foreach(Tile.Covering covering : tileOverlays.Keys) {
                if(covering != Tile.Covering.None){
                    Dictionary<Tile.TileType, Texture2D> newDictionary = new Dictionary<Tile.TileType, Texture2D>();
                    Texture2D overlay = tileOverlays[covering];
                    foreach(Tile.TileType type : dictionaryDefault.Keys){
                        Texture2D texture = dictionaryDefault[type];
                        if(TextureGenerator.AreTexturesSameSize(overlay, texture))
                        {
                            newDictionary[type] = TextureGenerator.GenerateTexture(texture, overlay);
                        }
                    }
                    tileTextureDictionary[covering] = newDictionary;
                } else {
                    tileTextureDictionary[Tile.Covering.None] = dictionaryDefault;
                }
                
            }
            
            // TODO: use this.Content to load your game content here
            Texture2D texture = playerTextureDictionary[Player.PlayerTextures.Body];
            player.hitbox = new Hitbox(new Vector2[] { new Vector2(0,0), new Vector2(texture.Width, 0), new Vector2(texture.Width, -texture.Height), new Vector2(0, -texture.Height) }, player.position, 2);
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
                    
                    Chunk c = world.LoadChunk((int)(player.position.X / (Tile.tilesize) / 64 + x), (int)(player.position.Y / (Tile.tilesize) / 64) + y);
                    
                    c.loaded = true;
                }
            }


            player.Process_((float)gameTime.ElapsedGameTime.TotalSeconds);
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
                                Texture2D tileTexture = tileTextureDictionary.GetValueOrDefault(tile.Covering)[tile.Type];
                                if(tileTexture == null){
                                    tileTexture = tileTextureDictionary.GetValueOrDefault(Tile.Covering.None)[tile.Type];
                                }
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
            _spriteBatch.Draw(texture, new Vector2(1920 - texture.Width / 2, 1080 - texture.Height / 2) / 2, null, Color.White, 0, new Vector2(), player.scale, SpriteEffects.None, 0);
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
