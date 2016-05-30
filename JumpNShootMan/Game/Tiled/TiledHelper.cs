using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
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
            var tiles = new TiledTile[9];
            // TODO change to loop, and prevent wrapping
            tiles[0] = tileLayer.GetTile(x, y - 1); 
            tiles[1] = tileLayer.GetTile(x - 1, y - 1);
            tiles[2] = tileLayer.GetTile(x, y + 1);

            tiles[3] = tileLayer.GetTile(x - 1, y);
            tiles[4] = tileLayer.GetTile(x, y);
            tiles[5] = tileLayer.GetTile(x + 1, y);

            tiles[6] = tileLayer.GetTile(x, y + 1); 
            tiles[7] = tileLayer.GetTile(x - 1, y + 1);
            tiles[8] = tileLayer.GetTile(x + 1, y + 1);

//            Debug.WriteLine(tiles[6].Id);
//            Debug.WriteLine(tiles[6].X);
//            Debug.WriteLine(tiles[6].Y);
//            System.Environment.Exit(0);

            return tiles;
        }

        public static Rectangle FindTileRectangle(Rectangle rectangleObject, TiledMap tileMap)
        {
            var center = rectangleObject.Center;
            var x = (int)Math.Floor((double) center.X/tileMap.TileWidth);
            var y = (int)Math.Floor((double) center.Y/tileMap.TileHeight);

            return new Rectangle(x, y, tileMap.TileWidth, tileMap.TileHeight);
        }
    }
}
