using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JumpNShootMan.Game;
using JumpNShootMan.Game.Tiled;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Maps.Tiled;
using MonoGame.Extended.Sprites;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended.Animations.SpriteSheets;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.DebugView;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using MonoGame.Extended.ViewportAdapters;

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
        private SpriteBatch mapSpriteBatch;
        public TiledMap tiledMap;
        public Song song;
        public SoundEffect playerDeathSting;
        public SoundEffect playerDeath;
        World world;
        Body manBody;
        private List<Body> platforms = new List<Body>();
        private Texture2D groundTexture;
        private DebugViewXNA physicsDebug;
        private Camera2D camera;

        public const float PIXELS_PER_METER = 68;

        // Create TileMap class // Needs to compare it's tiles to the Man, and anything else that could collide with them.
        // Add horizontal movement to man
        // Add jumping
        // 

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 704;   // set this value to the desired height of your window
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
            base.Initialize();
            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 1280 / PIXELS_PER_METER, 704 / PIXELS_PER_METER);
            camera = new Camera2D(viewportAdapter);
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            mapSpriteBatch = new SpriteBatch(GraphicsDevice);
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
            groundTexture = Content.Load<Texture2D>("Maps/level-1/brown-square");


            var animationGroup = Content.Load<SpriteSheetAnimationFactory>("Sprites/jumpman-animations");

            jumpNShootMan = new Man(new Vector2(playerStartObject.X, playerStartObject.Y - playerStartObject.Height), new SpriteSheetAnimator(animationGroup), this);
            jumpNShootMan.TileMap = tiledMap;

            this.song = Content.Load<Song>("Sounds/BGM");
            this.playerDeathSting = Content.Load<SoundEffect>("Sounds/player death sting");
            this.playerDeath = Content.Load<SoundEffect>("Sounds/playerdeath");
            MediaPlayer.Volume = 0.05f;
            MediaPlayer.IsRepeating = true;
            SoundEffect.MasterVolume = 0.1f;



            //MediaPlayer.Play(song);

            LoadPhysicsWorld();
            
        }

        protected void LoadPhysicsWorld()
        {
            FarseerPhysics.Settings.AllowSleep = true;
            FarseerPhysics.Settings.ContinuousPhysics = false;

            world = new World(new Vector2(0f, 9.82f));
            physicsDebug = new DebugViewXNA(world);
            physicsDebug.LoadContent(this.GraphicsDevice, this.Content);
            physicsDebug.AppendFlags(DebugViewFlags.Shape);
            physicsDebug.AppendFlags(DebugViewFlags.PolygonPoints);



            ConvertUnits.SetDisplayUnitToSimUnitRatio(PIXELS_PER_METER);
                        
            var platformLayer = TiledHelper.FindTileLayer("Platforms", tiledMap.TileLayers);
            if (platformLayer == null)
                throw new Exception("Could not find Platform Layer");


            // Add platform tiles
            foreach (TiledTile tile in platformLayer.Tiles)
            {
                // If the tile is blank, then it is not collidable.
                if (tile.Id == 0)
                    continue;

                var x = tile.X * tiledMap.TileWidth;
                var y = tile.Y * tiledMap.TileHeight;
                var rectangle = new RectangleF(x, y, tiledMap.TileWidth, tiledMap.TileHeight);

                var body = CreateRectangleBody(world, rectangle);
                platforms.Add(body);
            }

            // Add man
            //jumpNShootMan
            // TODO use floating point rectangles
            Debug.WriteLine(jumpNShootMan.Sprite.GetBoundingRectangle());
            var manRectangle = new RectangleF(jumpNShootMan.Position.X + jumpNShootMan.Sprite.GetBoundingRectangle().Width / 2, jumpNShootMan.Position.Y + jumpNShootMan.Sprite.GetBoundingRectangle().Height / 2, jumpNShootMan.Sprite.GetBoundingRectangle().Width, jumpNShootMan.Sprite.GetBoundingRectangle().Height);

            manBody = CreateRectangleBody(world, manRectangle, BodyType.Dynamic);
            jumpNShootMan.Body = manBody;


        }

        public static Body CreateRectangleBody(World world, RectangleF rectangle, BodyType bodyType = BodyType.Static, float density = 0, object userData = null)
        {
            var halfWidth = ConvertUnits.ToSimUnits(rectangle.Width / 2);
            var halfHeight = ConvertUnits.ToSimUnits(rectangle.Height / 2);
            var x = ConvertUnits.ToSimUnits(rectangle.X) + halfWidth;
            var y = ConvertUnits.ToSimUnits(rectangle.Y) + halfHeight;

            if (halfWidth <= 0)
                throw new ArgumentOutOfRangeException("width", "Width must be more than 0 meters");

            if (halfHeight <= 0)
                throw new ArgumentOutOfRangeException("height", "Height must be more than 0 meters");

            Body newBody = new Body(world, new Vector2(x, y), bodyType: bodyType);
            Vertices rectangleVertices = PolygonTools.CreateRectangle(halfWidth, halfHeight);
            PolygonShape rectangleShape = new PolygonShape(rectangleVertices, density);
            newBody.CreateFixture(rectangleShape, userData);

            return newBody;
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
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            //   Keyboard.GetState().IsKeyDown(Keys.Escape))
            //     Exit();

            world.Step(1 / 60f);
      //      Debug.WriteLine(new Vector2(manBody.Position.X * PIXELS_PER_METER, manBody.Position.Y * PIXELS_PER_METER));
            jumpNShootMan.Position = new Vector2(manBody.Position.X, manBody.Position.Y);

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

            var viewMatrix = camera.GetViewMatrix(Vector2.Zero);

            mapSpriteBatch.Begin();
            mapSpriteBatch.Draw(tiledMap, gameTime: gameTime);
            mapSpriteBatch.End();


            //spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null);
            spriteBatch.Begin(transformMatrix:viewMatrix);

          //      spriteBatch.Draw(tiledMap, gameTime: gameTime);
            //tiledMap.Draw(spriteBatch);
            //spriteBatch.Draw(jumpNShootMan.Sprite.TextureRegion.Texture, jumpNShootMan.Sprite.Position);

            //            var sprite = jumpNShootMan.Sprite;
            //  spriteBatch.Draw(sprite.TextureRegion.Texture, sprite.Position + sprite.Origin, sourceRectangle: sprite.TextureRegion.Bounds, color: sprite.Color * sprite.Alpha, rotation: sprite.Rotation, origin: sprite.Origin);

            Debug.WriteLine(viewMatrix);


            var scale = jumpNShootMan.Sprite.Scale;
            jumpNShootMan.Sprite.Scale = new Vector2(1 / PIXELS_PER_METER, 1 / PIXELS_PER_METER);
            //jumpNShootMan.Sprite.Scale = camera.
            spriteBatch.Draw(jumpNShootMan.Sprite);


            Matrix proj = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
            //Matrix view = camera.GetViewMatrix(Vector2.One);

            //Matrix view = Matrix.CreateTranslation(new Vector3(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height / 2.0f, 0.0f));
//            var view = camera.GetViewMatrix(Vector2.One);


            physicsDebug.RenderDebugData(ref proj, ref viewMatrix);


            foreach (Body body in platforms)
            {
                var groundSprite = new Sprite(groundTexture);
                groundSprite.Position = new Vector2(body.Position.X, body.Position.Y);
  //              spriteBatch.Draw(groundSprite);
            }

            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}