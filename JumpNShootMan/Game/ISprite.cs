using Microsoft.Xna.Framework;

namespace JumpNShootMan.Game

{
    public interface ISprite
    {
        void Initialize();
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
    }
}
