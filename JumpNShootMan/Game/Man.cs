using System;
using System.Diagnostics;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
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
//        Jumping
        JumpStart,
        Hang,
        Fall,
        FallEnd
    }

    public enum ManDirection
    {
        Left,
        Right
    }

    public class Man : AnimatedSprite
    {
        public int Speed { get; } = 1;
        public int Gravity { get; } = 3;

        public int footContactCount = 0;

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        public bool WasOnGround = false;

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
//        private const float MoveAcceleration = 7000.0f;
//        private const float MaxMoveSpeed = 600.0f;
//        private const float GroundDragFactor = 0.48f;
//        private const float AirDragFactor = 0.48f;

        private const float MoveSpeed = 80;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.25f;
        private const float JumpLaunchVelocity = -370.0f;
        private const float GravityAcceleration = 2000.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.1f;
        private const float JumpStopVelocity = -2f;
        private const float JumpVelocity = -4f;

        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const Buttons JumpButton = Buttons.A;
        private Game1 game;
        public Body Body;
        
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
            WasOnGround = isOnGround;
            GetInput(Keyboard.GetState(), GamePad.GetState(PlayerIndex.One));

            isOnGround = footContactCount > 0;

            Debug.WriteLine(isOnGround);

            ApplyPhysics(gameTime);

            /*  ApplyPhysics(gameTime);
              if (IsOnGround == false)
              {
                  State = ManState.Jumping;
              }
              */
            CheckBottomMapCollision();
            //Position = new Vector2(Position.X, Position.Y + 10);

            if (isOnGround == false)
            {
                //State = ManState.Jumping;
            }

            if (Body.LinearVelocity.Y > 0)
            {
                State = ManState.Fall;
            }

            if (WasOnGround == false && isOnGround == true)
            {
                State = ManState.FallEnd;
            }

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
//                    case ManState.Jumping:
//                        Animator.Play("Jump");
//                        break;
                    case ManState.JumpStart:
                        var animation = Animator.Play("JumpStart2");
                        animation.IsLooping = false;
                        
                        break;
                    case ManState.Hang:
                        Animator.Play("Hang");
                        break;
                    case ManState.Fall:
                        Animator.Play("Fall");
                        break;
                    case ManState.FallEnd:
                        Animator.Play("FallEnd");
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

            if (isOnGround)
            {
                State = (int)movement == 0 ? ManState.Idle : ManState.Walking;
            }

            

            Sprite.Effect = Direction == ManDirection.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }


        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            previousPosition = Position;

            var initialY = Body.LinearVelocity.Y;



            var newVelocity = new Vector2(movement * MoveSpeed * elapsed, Body.LinearVelocity.Y);


            Body.LinearVelocity = newVelocity;


            if (jumpTime > 0 && !isJumping)
            {
                JumpEnd();
            }

            if (Body.LinearVelocity.Y == 0)
            {
                jumpTime = 0;
            }

//            else
//            {
//                Body.GravityScale = 1;
//            }

            //       newVelocity.Y = MathHelper.Clamp(newVelocity.Y, -MaxFallSpeed, MaxFallSpeed);
            //            newVelocity.Y 
            DoJump(gameTime);
//






            // Apply pseudo-drag horizontally.
            //            if (IsOnGround)
            //                velocity.X *= GroundDragFactor;
            //            else
            //                velocity.X *= AirDragFactor;



            /*


            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
            velocity.Y = DoJump(velocity.Y, gameTime);

//            Debug.WriteLine(velocity);



            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
//            Debug.WriteLine(Position);
//            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

*/
        }

        private void CheckBottomMapCollision()
        {
            if (Position.Y > ConvertUnits.ToSimUnits(TileMap.HeightInPixels))
            {
                game.playerDeathSting.Play();
                game.playerDeath.Play();
                Body.Position = StartPosition();
                Body.LinearVelocity = Vector2.Zero;
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

            return new Vector2(ConvertUnits.ToSimUnits(playerStartObject.X), ConvertUnits.ToSimUnits(playerStartObject.Y - playerStartObject.Height - playerStartObject.Height / 2));

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
        private float DoJump(GameTime gameTime)
        {
            float velocityY = 0;

            // If the player wants to jump

                float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround && isJumping) || jumpTime > 0.0f)
                {
                    State = ManState.JumpStart;
                    // TODO Add sound
//                    if (jumpTime == 0.0f)
//                        jumpSound.Play();

                    jumpTime += elapsed;
                    // TODO add jump animation
//                    sprite.PlayAnimation(jumpAnimation);
                    isOnGround = false;
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    //velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                    //                    velocityY = JumpLaunchVelocity * (float)(jumpTime / MaxJumpTime);
                    
                    //velocityY = JumpLaunchVelocity * elapsed;

                    Body.LinearVelocity = new Vector2(Body.LinearVelocity.X, JumpVelocity);
                    //Body.ApplyLinearImpulse(new Vector2(0, -.7f));
                Debug.WriteLine(Body.LinearVelocity);
                }
                else if(jumpTime > MaxJumpTime)
                {
                    // Reached the apex of the jump
                    JumpEnd();
                }

            wasJumping = isJumping;

            Debug.WriteLine(velocityY);

            return velocityY;
        }

        public void JumpEnd()
        {
            State = ManState.Hang;
            jumpTime = 0.0f;
            Body.LinearVelocity = new Vector2(Body.LinearVelocity.X, JumpStopVelocity);
        }

        public bool OnFootSensorCollisionEvent(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            footContactCount++;
            return true;
        }

        public void OnFootSensorSeparationEvent(Fixture fixtureA, Fixture fixtureB)
        {
            footContactCount--;
        }
    }
}
