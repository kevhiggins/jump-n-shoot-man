using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JumpNShootMan.Game;
using JumpNShootMan.Game.Tiled;
using MonoGame.Extended.Maps.Tiled;

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
        public TiledMap tiledMap;
        public const int TILE_WIDTH = 32;
        public const int TILE_HEIGHT = 32;


        // Create TileMap class // Needs to compare it's tiles to the Man, and anything else that could collide with them.
        // Add horizontal movement to man
        // Add jumping
        // 

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 800;   // set this value to the desired height of your window
            graphics.ApplyChanges();
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
            Debug.WriteLine("TEST0");
            // TODO: Add your initialization logic here
         //   var texture = new Texture2D(GraphicsDevice, 20, 40);
         //   ColorTexture(texture, Color.Red);

            //            int[,] map1 = {{, , , EmptyTle, EmptyTile, EmptyTile, EmptyTile, EmptyTile, EmptyTile, EmptyTile, } };

            //            var mapPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Game\Maps\data\map-1.csv");
            //   var mapPath = Path.Combine(Environment.CurrentDirectory, @"Game\Maps\data\map-1.csv");

            //            Debug.WriteLine(mapPath);

            //    var tileMap = new TileMap(mapPath);
            //  tileMap.Initialize();

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
            tiledMap = Content.Load<TiledMap>("Maps/level-1/level-1");

            var gameObjectsLayer = tiledMap.GetObjectGroup("GameObjects");
            if (gameObjectsLayer == null)
            {
                throw new Exception("Could not find object layer");    
            }
            var playerStartObject = TiledHelper.FindObject("PlayerStart", gameObjectsLayer.Objects);
            if (playerStartObject == null)
            {
                throw new Exception("No player start point defined");
            }

            var manTexture = Content.Load<Texture2D>("Objects/player");

            jumpNShootMan = new Man(manTexture, new Vector2(playerStartObject.X, playerStartObject.Y - playerStartObject.Height));
            jumpNShootMan.TileMap = tiledMap;
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

            jumpNShootMan.Update(gameTime);
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
            tiledMap.Draw(spriteBatch);
            spriteBatch.Draw(jumpNShootMan.Texture, jumpNShootMan.Position);
            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}