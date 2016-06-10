using System;
using System.Diagnostics;
using JumpNShootMan.Game.Common;
using JumpNShootMan.Game.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Maps.Tiled;
using MonoGame.Extended.Animations.SpriteSheets;

namespace JumpNShootMan.Game
{
    public enum ManState
    {
        Idle,
        Walking,
        Jumping
    }

    public enum ManDirection
    {
        Left,
        Right
    }

    class Man : AnimatedSprite
    {
        public int Speed { get; } = 3;
        public int Gravity { get; } = 3;

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

        public ManDirection Direction { get; set; } = ManDirection.Right;
        
        private bool isJumping;
        private bool wasJumping;
        private double jumpTime;
        private int previousBottom;
        private Vector2 previousPosition;


        public TiledMap TileMap { get; set; }
        public Rectangle Bounds => new Rectangle((int)Math.Round(Position.X + 10), (int)Math.Round(Position.Y), (int)Sprite.GetBoundingRectangle().Width - 18, (int)Sprite.GetBoundingRectangle().Height);
        //public Rectangle Bounds => (Rectangle)BoundingBox;

        public ManState State { get; set; }
        private ManState lastState;

        // Constants for controling horizontal movement
        private const float MoveAcceleration = 7000.0f;
        private const float MaxMoveSpeed = 600.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.48f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.2f;
        private const float JumpLaunchVelocity = -370.0f;
        private const float GravityAcceleration = 2000.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.1f;

        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const Buttons JumpButton = Buttons.A;
        private Game1 game;
        
        public Man(Vector2 position, SpriteSheetAnimator animator, Game1 game) : base(position, animator)
        {
            this.game = game;
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

          /*  ApplyPhysics(gameTime);
            if (IsOnGround == false)
            {
                State = ManState.Jumping;
            }
            */
            //Position = new Vector2(Position.X, Position.Y + 10);
            UpdateAnimation();
          //  Debug.WriteLine(Position);
           // Debug.WriteLine(Bounds);
            Animator.Update(gameTime);
        }

        public void UpdateAnimation()
        {
            if (lastState != State)
            {
                switch (State)
                {
                    case ManState.Idle:
                        Animator.Play("Idle");
                        break;
                    case ManState.Walking:
                        Animator.Play("Walk");
                        break;
                    case ManState.Jumping:
                        Animator.Play("Jump");
                        break;
                }
            }
            lastState = State;
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

            if (movement < 0)
            {
                Direction = ManDirection.Left;
            } else if (movement > 0)
            {
                Direction = ManDirection.Right;
            }

            State = (int)movement == 0 ? ManState.Idle : ManState.Walking;

            Sprite.Effect = Direction == ManDirection.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }


        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            previousPosition = Position;



            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
            velocity.Y = DoJump(velocity.Y, gameTime);

//            Debug.WriteLine(velocity);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
//            Debug.WriteLine(Position);
//            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();


            CheckBottomMapCollision();
//          If the collision stopped us from moving, reset the velocity to zero.
//            if (Position.X == previousPosition.X)
//                velocity.X = 0;
//            if (Position.Y == previousPosition.Y)
//                velocity.Y = 0;

        }

        private void CheckBottomMapCollision()
        {
            if (Position.Y > TileMap.HeightInPixels)
            {
                game.playerDeathSting.Play();
                game.playerDeath.Play();
                Position = StartPosition();
                // Reset to start point
            }
        }

        public Vector2 StartPosition()
        {
            
            var gameObjectsLayer = TileMap.GetObjectGroup("GameObjects");
            if (gameObjectsLayer == null)
            {
                throw new Exception("Could not find object layer");
            }
            var playerStartObject = TiledHelper.FindObject("PlayerStart", gameObjectsLayer.Objects);
            if (playerStartObject == null)
            {
                throw new Exception("No player start point defined");
            }

            return new Vector2(playerStartObject.X, playerStartObject.Y - playerStartObject.Height);

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
                    //velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                    //                    velocityY = JumpLaunchVelocity * (float)(jumpTime / MaxJumpTime);
                    velocityY = JumpLaunchVelocity;
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

            var platformLayer = TiledHelper.FindTileLayer("Platforms", TileMap.TileLayers);
            if(platformLayer == null) 
                throw new Exception("Could not find Platform Layer");

            var tileRectangle = TiledHelper.FindTileRectangle(Bounds, TileMap);

            var adjacentTiles = TiledHelper.GetAdjacentTiles(tileRectangle.X, tileRectangle.Y, platformLayer);


            // Reset flag to search for ground collision.
            isOnGround = false;

            var tileWidth = platformLayer.TileWidth;
            var tileHeight = platformLayer.TileHeight;

            var obstructedOnX = false;
            var obstructedOnY = false;

            // For each potentially colliding tile,
            foreach (var adjacentTile in adjacentTiles)
            {

                // Tile is not collidable. Continue loop.
                if (adjacentTile == null || adjacentTile.Id == 0)
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
                        if (depth.Y < 0)
                        {
                            isOnGround = true;
                        }
                        // Resolve the collision along the Y axis.
                        Position = new Vector2(Position.X, bounds.Y + depth.Y);
                        obstructedOnY = true;

                        // Perform further collisions with the new bounds.
                        bounds = Bounds;
                    }
                    else /*if (collision == TileCollision.Impassable) */ // Ignore platforms.
                    {
                        // Resolve the collision along the X axis.
                        Position = new Vector2(Position.X + depth.X, Position.Y);
                        obstructedOnX = true;

                        // Perform further collisions with the new bounds.
                        bounds = Bounds;
                    }
                }
            }
//            Debug.WriteLine(isOnGround);

            if (obstructedOnX)
                velocity.X = 0;
            if (obstructedOnY)
            {
                velocity.Y = 0;
                if(isJumping)
                    jumpTime = MaxJumpTime;
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }
    }
}
