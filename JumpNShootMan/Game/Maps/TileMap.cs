using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace JumpNShootMan.Game.Maps
{
    public class TileMap : ISprite
    {
        private string mapPath;
        private Tile[,] tiles;
        private Microsoft.Xna.Framework.Game game;

        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }

        public TileMap(Microsoft.Xna.Framework.Game game, string mapPath)
        {
            this.game = game;
            this.mapPath = mapPath;
        }

        public void Initialize()
        {
            ReadMap();
            // int[,] map1 = {{, , , EmptyTle, EmptyTile, EmptyTile, EmptyTile, EmptyTile, EmptyTile, EmptyTile, } };
           // tiles = new Tile[,];
        }

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void Draw(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        protected void ReadMap()
        {
            var reader = new StreamReader(File.OpenRead(this.mapPath));
            var lines = new List<string>();

            while (!reader.EndOfStream)
            {
                lines.Add(reader.ReadLine());
            }

            var firstLine = lines.First();
            MapWidth = firstLine.Split(',').Length;
            MapHeight = lines.Count;

            Debug.WriteLine("TEST");

//            tiles = new Tile[MapHeight, MapWidth];


        }
    }
}
