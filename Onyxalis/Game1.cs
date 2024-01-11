using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Onyxalis.Objects.Worlds;
using Onyxalis.Objects.Entities;
using System;
using System.Collections.Generic;

namespace Onyxalis
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public static Random GameRandom = new Random();

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
                
                World world = World.CreateWorld();
                Player player = new Player();
                Vector2 spawnpoint = world.SpawnPlayerIn();

                 
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
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            /*
            switch (currentState)
            {
                case GameState.Lobby:
                    // Draw lobby screen
                    break;

                case GameState.Playing:
                    // Draw tiles
                    _spriteBatch.Draw(tileTexture, tilePosition1, Color.White);
                    _spriteBatch.Draw(tileTexture, tilePosition2, Color.White);
                    break;
            }*/

            _spriteBatch.End();

        

            base.Draw(gameTime);
        }
    }
}