using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JumpNShootMan.Game
{
    abstract class Sprite : ISprite
    {
        public Texture2D Texture { get; }
        public Vector2 Position { get; protected set; }

        protected Sprite(Texture2D texture, Vector2 position)
        {
            Texture = texture;
            Position = position;
        }

        public abstract void Initialize();

        public abstract void Draw(GameTime gameTime);


        public abstract void Update(GameTime gameTime);
    }
}
