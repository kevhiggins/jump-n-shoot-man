using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Shapes;
using MonoGame.Extended.Sprites;

namespace JumpNShootMan.Game
{
    abstract class AnimatedSprite : ISprite, IActorTarget
    {
        public Vector2 Position
        {
            get { return Sprite.Position; }
            set { Sprite.Position = value; }
        }

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        protected Vector2 velocity;
        public RectangleF BoundingBox => Sprite.GetBoundingRectangle();

        public SpriteSheetAnimator Animator;
        public Sprite Sprite;

        protected AnimatedSprite(Vector2 position, SpriteSheetAnimator animator)
        {
            Sprite = animator.Sprite;
            Position = position;
            Animator = animator;
        }

        public abstract void Initialize();

        public abstract void Draw(GameTime gameTime);

        public abstract void Update(GameTime gameTime);
        
        public void OnCollision(CollisionInfo collisionInfo)
        {
            throw new NotImplementedException();
        }
    }
}
