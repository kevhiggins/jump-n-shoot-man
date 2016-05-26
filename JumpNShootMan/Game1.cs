using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JumpNShootMan.Game;
using JumpNShootMan.Game.Maps;

namespace JumpNShootMan
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public const int EmptyTile = 0;
        public const int ObstructedTile = 0;
        private GraphicsDeviceManager graphics;
        private Man jumpNShootMan;
        private SpriteBatch spriteBatch;

        // Create TileMap class // Needs to compare it's tiles to the Man, and anything else that could collide with them.
        // Add horizontal movement to man
        // Add jumping
        // 

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            var texture = new Texture2D(GraphicsDevice, 20, 40);
            ColorTexture(texture, Color.Red);

            //            int[,] map1 = {{, , , EmptyTle, EmptyTile, EmptyTile, EmptyTile, EmptyTile, EmptyTile, EmptyTile, } };

//            var mapPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Game\Maps\data\map-1.csv");
         //   var mapPath = Path.Combine(Environment.CurrentDirectory, @"Game\Maps\data\map-1.csv");

//            Debug.WriteLine(mapPath);

        //    var tileMap = new TileMap(mapPath);
          //  tileMap.Initialize();
            
            jumpNShootMan = new Man(texture, new Vector2(50, 400));

            base.Initialize();
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        private static void ColorTexture(Texture2D texture, Color color)
        {
            // Fill the texture with red pixels
            var pixelCount = texture.Width*texture.Height;
            var colorData = new Color[pixelCount];
            for (var i = 0; i < pixelCount; i++)
            {
                colorData[i] = color;
            }
            texture.SetData(colorData);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(jumpNShootMan.Texture, jumpNShootMan.Position);
            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}