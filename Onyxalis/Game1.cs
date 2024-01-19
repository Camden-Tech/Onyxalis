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
using static Onyxalis.Objects.Tiles.Tile;

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
        public List<PartialChunk> partialChunks = new List<PartialChunk>();
        public static HashMap<Covering, HashMap<TileType, Texture2D>> tileTextureDictionary = new HashMap<Covering, HashMap<TileType, Texture2D>>();
        public static HashMap<Covering, Texture2D> tileOverlays = new HashMap<Covering, Texture2D>();
        public static HashMap<Player.PlayerTextures, Texture2D> playerTextureDictionary = new HashMap<Player.PlayerTextures, Texture2D>();
        public static HashMap<TileType, Texture2D[,]> multiTilePieces = new HashMap<TileType, Texture2D[,]>();
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
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            HashMap<TileType, Texture2D> dictionaryDefault = new HashMap<TileType, Texture2D> //Create json file reader instead
            {
                { TileType.DIRT1, Content.Load<Texture2D>("Tiles/Dirt") },
                { TileType.DIRT2, Content.Load<Texture2D>("Tiles/DirtTwo") },
                { TileType.GRASS, Content.Load<Texture2D>("Tiles/Grass") },
                { TileType.DIRT3, Content.Load<Texture2D>("Tiles/DirtThree") },
                { TileType.GRASS2, Content.Load<Texture2D>("Tiles/GrassTwo") },
                { TileType.STONE, Content.Load<Texture2D>("Tiles/Stone") },
                { TileType.DEEPROCK1, Content.Load<Texture2D>("Tiles/DeeprockOne") },
                { TileType.DEEPROCK2, Content.Load<Texture2D>("Tiles/DeeprockTwo") },
                { TileType.DEEPROCK3, Content.Load<Texture2D>("Tiles/DeeprockThree") },
                { TileType.DEEPROCK4, Content.Load<Texture2D>("Tiles/DeeprockFour") },
                { TileType.PERMAFROST1, Content.Load<Texture2D>("Tiles/PermafrostOne") },
                { TileType.PERMAFROST2, Content.Load<Texture2D>("Tiles/PermafrostTwo") },
                { TileType.PERMAFROST3, Content.Load<Texture2D>("Tiles/PermafrostThree") },
                { TileType.PERMAFROST4, Content.Load<Texture2D>("Tiles/PermafrostFour") },
                { TileType.COPPERDEEPROCK, Content.Load<Texture2D>("Tiles/CopperDeeprock") },
                { TileType.SNOW1, Content.Load<Texture2D>("Tiles/SnowOne") },
                { TileType.SNOW2, Content.Load<Texture2D>("Tiles/SnowTwo") },
                { TileType.SNOW3, Content.Load<Texture2D>("Tiles/SnowThree") },
                { TileType.SNOW4, Content.Load<Texture2D>("Tiles/SnowFour") },
                { TileType.SAND1, Content.Load<Texture2D>("Tiles/SandOne") },
                { TileType.SAND2, Content.Load<Texture2D>("Tiles/SandTwo") },
                { TileType.SAND3, Content.Load<Texture2D>("Tiles/SandThree") },
                { TileType.SAND4, Content.Load<Texture2D>("Tiles/SandFour") },
                { TileType.DIRT4, Content.Load<Texture2D>("Tiles/DirtFour") }
            };
            playerTextureDictionary.Add(Player.PlayerTextures.Body, Content.Load<Texture2D>("Player/BeautifulPlayerCharacter"));
            
            tileTextureDictionary[Covering.NONE] = dictionaryDefault;
            tileOverlays[Covering.MOSS] = Content.Load<Texture2D>("Overlays/Moss1");
            foreach (Covering covering in tileOverlays.Keys) {
                if(covering != Covering.NONE){
                    HashMap<TileType, Texture2D> newDictionary = new HashMap<TileType, Texture2D>();
                    Texture2D overlay = tileOverlays[covering];
                    foreach(TileType type in dictionaryDefault.Keys){
                        Texture2D underlay = dictionaryDefault[type];
                         newDictionary[type] = TextureGenerator.GenerateTexture(underlay, overlay, GraphicsDevice);
                        
                    }
                    tileTextureDictionary[covering] = newDictionary;
                }
                
            }

            HashMap<TileType, Texture2D> multiTiles = new HashMap<TileType, Texture2D>
            {
                { TileType.TREESTUMP, Content.Load<Texture2D>("Multitiles/TreeStump") },
                { TileType.TREESTALK, Content.Load<Texture2D>("Multitiles/Stalk") },
                { TileType.TREETOP, Content.Load<Texture2D>("Multitiles/Top") },
                { TileType.SHRUB, Content.Load<Texture2D>("Multitiles/Shrub") },
                { TileType.SHORTGRASS, Content.Load<Texture2D>("Multitiles/ShortGrass") },
                { TileType.LONGGRASS, Content.Load<Texture2D>("Multitiles/TallGrass") }
            };

            foreach (TileType type in multiTiles.Keys)
            {
                Texture2D multiTileTexture = multiTiles[type];
                Texture2D[,] pieces = TextureGenerator.BreakIntoPieces(multiTileTexture, GraphicsDevice);
                if (pieces[0,0] == null)
                {

                }
                multiTilePieces[type] = pieces;
            }


            


        // TODO: use this.Content to load your game content here
        Texture2D texture = playerTextureDictionary[Player.PlayerTextures.Body];
            player.hitbox = new Hitbox(new Vector2[] { new Vector2(0,0), new Vector2(texture.Width, 0), new Vector2(texture.Width, -texture.Height), new Vector2(0, -texture.Height) }, player.position, 2);
            BeginGameCreation();
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
                                Texture2D tileTexture;
                                if (!tile.multiTile)
                                {
                                    tileTexture = tileTextureDictionary[tile.covering][tile.Type];

                                    if (tileTexture == null)
                                    {
                                        tileTexture = tileTextureDictionary[Tile.Covering.NONE][tile.Type];
                                    }
                                } else
                                {
                                    tileTexture = multiTilePieces[tile.Type][tile.piecePos.x, tile.piecePos.y];
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
