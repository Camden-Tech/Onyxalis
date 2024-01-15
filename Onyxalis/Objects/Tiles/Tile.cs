using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Tiles
{
    public class Tile
    {
        public const int tilesize = 16;
        public enum TileType
        {
            GRASS,
            DIRT1,
            DIRT2,
            STONE,
            WOOD
        }

        //Each tile is 8x8
        public int x;
        public int y;
        public (int chunkX, int chunkY) chunkPos;
        public int rotation;
        public TileType Type;


    }
}
