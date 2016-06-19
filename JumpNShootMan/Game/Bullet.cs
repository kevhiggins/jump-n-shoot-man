using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;

namespace JumpNShootMan.Game
{
    class Bullet : Sprite
    {
        public Body Body;

        public Bullet(TextureRegion2D textureRegion) : base(textureRegion)
        {
        }

        public Bullet(Texture2D texture) : base(texture)
        {
        }

        public bool OnCollisionEvent(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (fixtureB.UserData is Man || fixtureB.UserData is Bullet)
            {
                return false;
            }
            fixtureA.Body.Dispose();
            return false;
        }
    }
}
