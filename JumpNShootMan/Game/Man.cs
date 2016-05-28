using System;
using System.Diagnostics;
using JumpNShootMan.Game.Common;
using JumpNShootMan.Game.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Maps.Tiled;

namespace JumpNShootMan.Game
{
    class Man : Sprite
    {
        public int Speed { get; } = 3;
        public int Gravity { get; } = 3;
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        private Vector2 velocity;

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        /// <summary>
        /// Current user movement input.
        /// </summary>
        private float movement;

        private bool isJumping;
        private bool wasJumping;
        private double jumpTime;
        private int previousBottom;


        public TiledMap TileMap { get; set; }
        public Rectangle Bounds => new Rectangle((int)Position.X + 8, (int)Position.Y, Texture.Width - 8, Texture.Height);



        // Constants for controling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;

        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const Buttons JumpButton = Buttons.A;


        public Man (Texture2D texture, Vector2 position) : base(texture, position)
        {
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override void Draw(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            GetInput(Keyboard.GetState(), GamePad.GetState(PlayerIndex.One));

            ApplyPhysics(gameTime);
        }


        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void GetInput(
            KeyboardState keyboardState,
            GamePadState gamePadState
            )
        {
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
            }

            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W);
        }


        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    // TODO Add sound
//                    if (jumpTime == 0.0f)
//                        jumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    // TODO add jump animation
//                    sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = Bounds;

//            var xTilePosition = (int)Math.Floor(Position.X /Game1.TILE_WIDTH);
//            var yTilePosition = (int)Math.Floor(Position.Y/Game1.TILE_WIDTH);
            var platformLayer = TiledHelper.FindTileLayer("Platforms", TileMap.TileLayers);
            if(platformLayer == null) 
                throw new Exception("Could not find Platform Layer");

            var tileRectangle = TiledHelper.FindTileRectangle(Bounds, TileMap);

            //            var currentTile = platformLayer.GetTile(tileRectangle.X, tileRectangle.Y);
            //            if (currentTile == null)
            //            {
            //                return;
            //            }

            var adjacentTiles = TiledHelper.GetAdjacentTiles(tileRectangle.X, tileRectangle.Y, platformLayer);


            // Reset flag to search for ground collision.
            isOnGround = false;

            var tileWidth = platformLayer.TileWidth;
            var tileHeight = platformLayer.TileHeight;

            // For each potentially colliding tile,
            foreach (var adjacentTile in adjacentTiles)
            {
                // Tile is not collidable. Continue loop.
                if (adjacentTile.Id == 0)
                    continue;
                

                // Determine collision depth (with direction) and magnitude.
                Rectangle tileBounds = new Rectangle(adjacentTile.X * tileWidth, adjacentTile.Y * tileHeight, tileWidth, tileHeight);
                Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                if (depth != Vector2.Zero)
                {
                    float absDepthX = Math.Abs(depth.X);
                    float absDepthY = Math.Abs(depth.Y);

                    // Resolve the collision along the shallow axis.
                    if (absDepthY < absDepthX /*|| collision == TileCollision.Platform */)
                    {
                        // If we crossed the top of a tile, we are on the ground.
                        if (previousBottom <= tileBounds.Top)
                            isOnGround = true;

                        // Ignore platforms, unless we are on the ground.
                        if (/*collision == TileCollision.Impassable || IsOnGround */ true)
                        {
                            // Resolve the collision along the Y axis.
                            Position = new Vector2(Position.X, Position.Y + depth.Y);

                            // Perform further collisions with the new bounds.
                            bounds = Bounds;
                        }
                    }
                    else /*if (collision == TileCollision.Impassable) */ // Ignore platforms.
                    {
                        // Resolve the collision along the X axis.
                        Position = new Vector2(Position.X + depth.X, Position.Y);

                        // Perform further collisions with the new bounds.
                        bounds = Bounds;
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }
    }
}
