using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class Tile
    {
        float distanceFromNearestTransparentTile = 0;
        public (int x, int y) closestTransparentTileLocation;
        public const int tilesize = 16;
        public enum TileType
        {
            GRASS,
            DIRT,
            STONE,
            WOOD
        }

        //Each tile is 8x8
        public int x;
        public int y;
        public TileType Type;


        public float postProcessingTileDistance(Tile[] adjacentTiles)
        {
            float closestDistance = 50;
            for (int i = 0; i < adjacentTiles.Length; i++)
            {
                Tile tile = adjacentTiles[i];
                if (tile == null)
                {
                    closestTransparentTileLocation = (tile.x, tile.y);
                    return 0;
                }
                if (closestDistance > tile.distanceFromNearestTransparentTile)
                {
                    closestTransparentTileLocation = tile.closestTransparentTileLocation;
                }
            }

                distanceFromNearestTransparentTile = MathF.Sqrt(MathF.Pow(closestTransparentTileLocation.x - x, 2) + MathF.Pow(closestTransparentTileLocation.y - y, 2));
                return distanceFromNearestTransparentTile;
        }
    }
}
