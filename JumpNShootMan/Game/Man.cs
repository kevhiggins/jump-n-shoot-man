using System;
using System.Diagnostics;
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
        public Vector2 Velocity { get; private set; } = new Vector2(0, 1);
        private bool isJumping = false;

        public TiledMap TileMap { get; set; }

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
            var keyboardState = Keyboard.GetState();

            var newVelocity = Velocity;
//            var xVelocity = 0;
//        
//
//            if (keyboardState.IsKeyDown(Keys.Right))
//            {
//                xVelocity += Speed;
//            }
//            if (keyboardState.IsKeyDown(Keys.Left))
//            {
//                xVelocity -= Speed;
//            }


            var yAcceleration = (float) 0;

            if (keyboardState.IsKeyDown(Keys.Space))
            {
                if (isJumping == false)
                {
                    isJumping = true;
                    yAcceleration = -20;
                }
            }

//            newVelocity.X = xVelocity;
            

            var acceleration = new Vector2(0, (float).2 + yAcceleration);
            Velocity += acceleration;
            Position += Velocity;

            var xTilePosition = (int)Math.Floor(Position.X / Game1.TILE_WIDTH);
            var yTilePosition = (int)Math.Floor(Position.Y/Game1.TILE_WIDTH);
            var platformLayer = TiledHelper.FindTileLayer("Platforms", TileMap.TileLayers);
            if(platformLayer == null) 
                throw new Exception("Could not find Platform Layer");

            var currentTile = platformLayer.GetTile(xTilePosition, yTilePosition);
            if (currentTile == null)
            {
                return;
            }
            Debug.WriteLine(currentTile.X);
            Debug.WriteLine(currentTile.Y);


            // x position / 32
            // y position / 32

            //Velocity += acceleration * time;
            //Position += Velocity*time;

            // this.Position = Position + new Vector2(-Speed, 0);



            //Position = new Vector2(Position.X + xOffset, Position.Y);

            // Acceleration


        }
    }
}
