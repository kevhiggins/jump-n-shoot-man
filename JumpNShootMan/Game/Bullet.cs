using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;

namespace JumpNShootMan.Game
{
    public class Bullet : Sprite
    {
        public Body Body;
        public Game1 game;

        public Bullet(TextureRegion2D textureRegion) : base(textureRegion)
        {
        }

        public Bullet(Game1 game, Texture2D texture) : base(texture)
        {
            this.game = game;
        }

        public bool OnCollisionEvent(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (fixtureB.UserData is Man || fixtureB.UserData is Bullet)
            {
                return false;
            }
            
            fixtureA.Body.Dispose();
            game.bullets.Remove(this);
            return false;
        }

        public void Update()
        {
            Position = Body.Position;
        }
    }
}
