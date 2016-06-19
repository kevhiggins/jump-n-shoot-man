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
using JumpNShootMan.Game.Maps;
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
        public World world;
        public List<Bullet> bullets;

        Body manBody;
        private List<Body> platforms = new List<Body>();
        private Texture2D groundTexture;
        private DebugViewXNA physicsDebug;
        private Camera2D camera;

        public const float PIXELS_PER_METER = 68 * 3;

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
            bullets = new List<Bullet>();
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


            //animationGroup.

            Debug.WriteLine(playerStartObject.X);
            Debug.WriteLine(playerStartObject.Y);

            jumpNShootMan = new Man(new Vector2(playerStartObject.X, playerStartObject.Y - playerStartObject.Height - playerStartObject.Height / 2), new SpriteSheetAnimator(animationGroup), this);
            jumpNShootMan.TileMap = tiledMap;

            this.song = Content.Load<Song>("Sounds/BGM");
            this.playerDeathSting = Content.Load<SoundEffect>("Sounds/player death sting");
            this.playerDeath = Content.Load<SoundEffect>("Sounds/playerdeath");
            MediaPlayer.Volume = 0.05f;
            MediaPlayer.IsRepeating = true;
            SoundEffect.MasterVolume = 0.1f;
            jumpNShootMan.Sprite.Scale = new Vector2(3, 3);


            //MediaPlayer.Play(song);

            LoadPhysicsWorld();
            
        }

        protected void LoadPhysicsWorld()
        {
            FarseerPhysics.Settings.AllowSleep = true;
//            FarseerPhysics.Settings.ContinuousPhysics = false;

//            Settings.VelocityIterations = 100;
//            Settings.PositionIterations = 100;
//            Settings.TOIPositionIterations = 100;
//            Settings.TOIVelocityIterations = 100;

            world = new World(new Vector2(0f, 14f));
            //Settings.MaxPolygonVertices = 8;
//            Settings.VelocityIterations = 200;
            
//            Settings.VelocityIterations = 100;
//            Settings.PositionIterations = 100;
//            Settings.PositionIterations = 128;
//            Settings.VelocityIterations = 128;

            
            
            physicsDebug = new DebugViewXNA(world);
            physicsDebug.LoadContent(this.GraphicsDevice, this.Content);
            physicsDebug.AppendFlags(DebugViewFlags.Shape);
            physicsDebug.AppendFlags(DebugViewFlags.PolygonPoints);



            ConvertUnits.SetDisplayUnitToSimUnitRatio(PIXELS_PER_METER);
                        
            var platformLayer = TiledHelper.FindTileLayer("Platforms", tiledMap.TileLayers);
            if (platformLayer == null)
                throw new Exception("Could not find Platform Layer");


            var edgeTracer = new EdgeTracer();
            edgeTracer.TraceEdges(tiledMap, platformLayer);


            /*

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
            */
            foreach (var pointPair in edgeTracer.topEdges)
            {
                CreateEdge(world, tiledMap, pointPair.Item1, pointPair.Item2, TileSide.Top);
            }

            foreach (var pointPair in edgeTracer.bottomEdges)
            {
                CreateEdge(world, tiledMap, pointPair.Item1, pointPair.Item2, TileSide.Bottom);
            }

            foreach (var pointPair in edgeTracer.leftEdges)
            {
                CreateEdge(world, tiledMap, pointPair.Item1, pointPair.Item2, TileSide.Left);
            }

            foreach (var pointPair in edgeTracer.rightEdges)
            {
                CreateEdge(world, tiledMap, pointPair.Item1, pointPair.Item2, TileSide.Right);
            }

            // Add man
            //jumpNShootMan
            // TODO use floating point rectangles
            var manRectangle = new RectangleF(jumpNShootMan.Position.X + jumpNShootMan.Sprite.GetBoundingRectangle().Width / 2, jumpNShootMan.Position.Y + jumpNShootMan.Sprite.GetBoundingRectangle().Height / 2, jumpNShootMan.Sprite.GetBoundingRectangle().Width / 2 - 10, jumpNShootMan.Sprite.GetBoundingRectangle().Height);

            manBody = CreateManBody(jumpNShootMan, world, manRectangle, BodyType.Dynamic);
            jumpNShootMan.Body = manBody;


        }

        public static void CreateEdge(World world, TiledMap tiledMap, Point startPoint, Point endPoint, TileSide side)
        {
            Debug.WriteLine(startPoint);
            Debug.WriteLine(endPoint);

            var a = new Point(startPoint.X, startPoint.Y);
            var b = new Point(endPoint.X, endPoint.Y);
            var bodyStart = new Vector2();
            
            switch (side)
            {
                case TileSide.Top:
                    // Extend on x axis one square.
                    b.X += 1;
                    break;
                case TileSide.Bottom:
                    // Extend on x axis one square.
                    b.X += 1;

                    // Move to the bottom of the block.
                    a.Y += 1;

                    bodyStart.Y = ConvertUnits.ToSimUnits(tiledMap.TileHeight);
                    break;
                case TileSide.Left:
                    b.Y += 1;
                    break;
                case TileSide.Right:
                    b.Y += 1;
                    bodyStart.X = ConvertUnits.ToSimUnits(tiledMap.TileWidth);
                    break;
            }

            var start = new Vector2(ConvertUnits.ToSimUnits(startPoint.X * tiledMap.TileWidth), ConvertUnits.ToSimUnits(startPoint.Y * tiledMap.TileHeight));
            var end = new Vector2(ConvertUnits.ToSimUnits(b.X * tiledMap.TileWidth), ConvertUnits.ToSimUnits(b.Y * tiledMap.TileHeight));

//            Debug.WriteLine(start);
//            Debug.WriteLine(end);

            Body newBody = new Body(world, bodyStart);
            var edgeShape = new EdgeShape(start, end);
            var fixture = newBody.CreateFixture(edgeShape);
            fixture.Restitution = 0;
            fixture.Friction = 0;
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
            var fixture = newBody.CreateFixture(rectangleShape, userData);
            fixture.Friction = 0;

            return newBody;
        }

        public static Body CreateManBody(Man man, World world, RectangleF rectangle, BodyType bodyType = BodyType.Static, float density = 0)
        {
            var halfWidth = ConvertUnits.ToSimUnits(rectangle.Width / 2);
            var halfHeight = ConvertUnits.ToSimUnits(rectangle.Height / 2 - 3.5);
            var x = ConvertUnits.ToSimUnits(rectangle.X) + halfWidth;
            var y = ConvertUnits.ToSimUnits(rectangle.Y) + halfHeight;

            if (halfWidth <= 0)
                throw new ArgumentOutOfRangeException("width", "Width must be more than 0 meters");

            if (halfHeight <= 0)
                throw new ArgumentOutOfRangeException("height", "Height must be more than 0 meters");

            Body newBody = new Body(world, new Vector2(x, y), bodyType: bodyType);

            /*

            var vertices = new Vertices();
            var trimCornerAmount = .03f;
            var slopeMultiplierX = 10;
            var slopeMultiplierY = 0.5f;

            var trimCornerAmountSide = .03f;
            var slopeMultiplierXSide = 0.5f;
            var slopeMultiplerYSide = 20;



            // Upper left
            vertices.Add(new Vector2(-halfWidth - trimCornerAmountSide * slopeMultiplierXSide, -halfHeight + trimCornerAmountSide * slopeMultiplerYSide));
            vertices.Add(new Vector2(-halfWidth, -halfHeight + trimCornerAmount * slopeMultiplierY));
            vertices.Add(new Vector2(-halfWidth + trimCornerAmount * slopeMultiplierX, -halfHeight));
            // Upper right
            vertices.Add(new Vector2(halfWidth + trimCornerAmountSide * slopeMultiplierXSide, -halfHeight + trimCornerAmountSide * slopeMultiplerYSide));
            vertices.Add(new Vector2(halfWidth, -halfHeight + trimCornerAmount * slopeMultiplierY));
            vertices.Add(new Vector2(halfWidth - trimCornerAmount * slopeMultiplierX, -halfHeight));
            // Lower right
            vertices.Add(new Vector2(halfWidth + trimCornerAmountSide * slopeMultiplierXSide, halfHeight - trimCornerAmountSide * slopeMultiplerYSide));
            vertices.Add(new Vector2(halfWidth, halfHeight - trimCornerAmount * slopeMultiplierY));
            vertices.Add(new Vector2(halfWidth - trimCornerAmount * slopeMultiplierX, halfHeight));
            // Lower left
            vertices.Add(new Vector2(-halfWidth - trimCornerAmountSide * slopeMultiplierXSide, halfHeight - trimCornerAmountSide * slopeMultiplerYSide));
            vertices.Add(new Vector2(-halfWidth, halfHeight - trimCornerAmount * slopeMultiplierY));
            vertices.Add(new Vector2(-halfWidth + trimCornerAmount * slopeMultiplierX, halfHeight));

            */
            var vertices = PolygonTools.CreateRectangle(halfWidth, halfHeight);

            var shape = new PolygonShape(vertices, density);
            var fixture = newBody.CreateFixture(shape, man);
            fixture.Restitution = 0;
            

            // TODO check if a block is to the right or left of the sensor collision, and don't allow jumping if so.

            var sensorVertices = new Vertices();
            var sensorHeight = .01f;
            var sensorWidthPadding = .9f;
            // Upper left
            sensorVertices.Add(new Vector2(-halfWidth * sensorWidthPadding, halfHeight));
            // Lower left
            sensorVertices.Add(new Vector2(-halfWidth * sensorWidthPadding, halfHeight + sensorHeight));
            // Lower right
            sensorVertices.Add(new Vector2(halfWidth * sensorWidthPadding, halfHeight + sensorHeight));
            // Upper right
            sensorVertices.Add(new Vector2(halfWidth * sensorWidthPadding, halfHeight));

            var footSensorShape = new PolygonShape(sensorVertices, density);
            var sensor = newBody.CreateFixture(footSensorShape, man);
            sensor.IsSensor = true;


            sensor.OnCollision = new OnCollisionEventHandler(man.OnFootSensorCollisionEvent);
            sensor.OnSeparation = new OnSeparationEventHandler(man.OnFootSensorSeparationEvent);

            newBody.OnCollision += new OnCollisionEventHandler(man.OnCollisionEvent);

            newBody.IsBullet = true;
//            newBody.LinearDamping = 2;

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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
               Keyboard.GetState().IsKeyDown(Keys.Escape))
                 Exit();

            world.Step(1 / 60f);

            jumpNShootMan.Position = new Vector2(manBody.Position.X, manBody.Position.Y);

            jumpNShootMan.Update(gameTime);

            foreach (var bullet in bullets)
            {
                bullet.Update();
            }

            camera.Zoom = 1;

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        public static void ColorTexture(Texture2D texture, Color color)
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
            spriteBatch.Begin(transformMatrix:viewMatrix, samplerState: SamplerState.PointClamp);

          //      spriteBatch.Draw(tiledMap, gameTime: gameTime);
            //tiledMap.Draw(spriteBatch);
            //spriteBatch.Draw(jumpNShootMan.Sprite.TextureRegion.Texture, jumpNShootMan.Sprite.Position);

            //            var sprite = jumpNShootMan.Sprite;
            //  spriteBatch.Draw(sprite.TextureRegion.Texture, sprite.Position + sprite.Origin, sourceRectangle: sprite.TextureRegion.Bounds, color: sprite.Color * sprite.Alpha, rotation: sprite.Rotation, origin: sprite.Origin);




            var scale = jumpNShootMan.Sprite.Scale;
            jumpNShootMan.Sprite.Scale = new Vector2(1 / PIXELS_PER_METER, 1 / PIXELS_PER_METER) * new Vector2(3, 3);
            //jumpNShootMan.Sprite.Scale = camera.
            spriteBatch.Draw(jumpNShootMan.Sprite);

            foreach (var bullet in bullets)
            {
                bullet.Scale = new Vector2(1 / PIXELS_PER_METER, 1 / PIXELS_PER_METER) * new Vector2(3, 3);
                spriteBatch.Draw(bullet);
            }


            Matrix proj = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
            //Matrix view = camera.GetViewMatrix(Vector2.One);

            //Matrix view = Matrix.CreateTranslation(new Vector3(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height / 2.0f, 0.0f));
//            var view = camera.GetViewMatrix(Vector2.One);


//            physicsDebug.RenderDebugData(ref proj, ref viewMatrix);


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