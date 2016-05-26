using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JumpNShootMan.Game.Maps
{
    class Tile : Sprite
    {
        public bool IsPassable { get; set; } = true;

        public Tile(Texture2D texture, Vector2 position) : base(texture, position)
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
            throw new NotImplementedException();
        }
    }
}
