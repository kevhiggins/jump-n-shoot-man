using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Maps.Tiled;

namespace JumpNShootMan.Game.Maps
{
    class EdgeTracer
    {
        // Create Tuple lists of the edge points of all non 0 ID tiles for the layer.
        public void TraceEdges(TiledMap map, TiledTileLayer layer)
        {

            var topEdges = new List<Tuple<Point, Point>>();
            var bottomEdges = new List<Tuple<Point, Point>>();
            var leftEdges = new List<Tuple<Point, Point>>();
            var rightEdges = new List<Tuple<Point, Point>>();

            // Find top and bottom edges
            for (var y = 0; y < map.Height; y++)
            {

                TiledTile previousTile = null;
                var previousTileHasTopEdge = false;
                var previousTileHasBottomEdge = false;
                Point startTopEdge = Point.Zero;
                Point startBottomEdge = Point.Zero;
                
                for (var x = 0; x <= map.Width; x++)
                {
                    var currentTile = GetTile(layer, x, y);

                    ProcessEdge(layer, currentTile, GetTile(layer, x, y - 1), previousTile, ref previousTileHasTopEdge, ref startTopEdge, topEdges);
                    ProcessEdge(layer, currentTile, GetTile(layer, x, y + 1), previousTile, ref previousTileHasBottomEdge, ref startBottomEdge, bottomEdges);
                    previousTile = currentTile;
                }
            }

            // Find left and right edges
            for (var x = 0; x < map.Width; x++)
            {

                TiledTile previousTile = null;
                var previousTileHasLeftEdge = false;
                var previousTileHasRightEdge = false;
                Point startLeftEdge = Point.Zero;
                Point startRightEdge = Point.Zero;

                for (var y = 0; y <= map.Height; y++)
                {
                    var currentTile = GetTile(layer, x, y);

                    ProcessEdge(layer, currentTile, GetTile(layer, x - 1, y), previousTile, ref previousTileHasLeftEdge, ref startLeftEdge, leftEdges);
                    ProcessEdge(layer, currentTile, GetTile(layer, x + 1, y), previousTile, ref previousTileHasRightEdge, ref startRightEdge, rightEdges);
                    previousTile = currentTile;
                }
            }

        }

        public void ProcessEdge(TiledTileLayer layer, TiledTile currentTile, TiledTile targetTile, TiledTile previousTile, ref bool previousTileHasEdge, ref Point startEdgePoint, List<Tuple<Point, Point>> edges)
        {
            var hasEdge = HasEdge(currentTile, targetTile);

            // Cases:
            // Previous tile has an edge, and so does this one => Continue
            // Previous tile does not have an edge, and neither does this one => continue
            // Previous tile does not have edge, but this one does => Start an edge
            // Previous tile has an edge, but this one does not => end edge (Draw the edge)

            // If the state of the edge is the same, then continue.
            if (previousTileHasEdge == hasEdge)
            {
            }
            else if (hasEdge)
            {
                // Start the edge (Initialize the point, and set previousTileEdge to true)
                startEdgePoint = new Point(currentTile.X, currentTile.Y);
                previousTileHasEdge = true;
            }
            else
            {
                // Add the edge to the list, and set previousTileEdge to false
                edges.Add(new Tuple<Point, Point>(startEdgePoint, new Point(previousTile.X, previousTile.Y)));
                previousTileHasEdge = false;
            }
        }

        public bool HasEdge(TiledTile currentTile, TiledTile targetTile)
        {
            // If not collidable, return false
            if (currentTile == null || currentTile.Id == 0)
                return false;

            // If target tile does not exist, then there is an edge.
            if (targetTile == null)
                return true;

            // Return whether true if the target tile is not collidable.
            return targetTile.Id == 0;
        }

        public TiledTile GetTile(TiledTileLayer layer, int x, int y)
        {
            if (layer.Width <= x || layer.Height <= y)
            {
                return null;
            }

            return layer.GetTile(x, y);
        }
    }
}
