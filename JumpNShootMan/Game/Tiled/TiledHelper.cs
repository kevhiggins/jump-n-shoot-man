using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended.Maps.Tiled;

namespace JumpNShootMan.Game.Tiled
{
    public static class TiledHelper
    {
        public static TiledObject FindObject(string name, TiledObject[] tileObjects)
        {
            foreach (var tileObject in tileObjects)
            {
                if (tileObject.Name == name)
                {
                    return tileObject;
                }
            }
            return null;
        }

        public static TiledTileLayer FindTileLayer(string name, IEnumerable<TiledTileLayer> tileLayers)
        {
            foreach (var tileLayer in tileLayers)
            {
                if (tileLayer.Name == name)
                {
                    return tileLayer;
                }
            }
            return null;
        }

        public static TiledTile[] GetAdjacentTiles(int x, int y, TiledTileLayer tileLayer)
        {
            // Offset the position to the upper left most tile
            x--;
            y--;

            var tiles = new TiledTile[8];
            

            for (int i = x; i < x + 3; i++)
            {
                for (int j = y; j < y + 3; j++)
                {

                    
                }
            }
        }
    }
}
