using AStarNavigator;
using Microsoft.Xna.Framework;
using Onyxalis.Objects.Math;
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
            GRASS = 0,
            GRASS2 = 1,

            DIRT1 = 2,
            DIRT2 = 3,
            DIRT3 = 4,
            DIRT4 = 5,

            STONE = 6,

            DEEPROCK1 = 7,
            DEEPROCK2 = 8,
            DEEPROCK3 = 9,
            DEEPROCK4 = 10,

            PERMAFROST1 = 11,
            PERMAFROST2 = 12,
            PERMAFROST3 = 13,
            PERMAFROST4 = 14,

            SAND1 = 15,
            SAND2 = 16,
            SAND3 = 17,
            SAND4 = 18,

            SNOW1 = 19,
            SNOW2 = 20,
            SNOW3 = 21,
            SNOW4 = 22,

            SHRUB = 23,

            SHORTGRASS = 24,
            TALLGRASS = 25,
            
            COPPERDEEPROCK = 26,

            WOOD = 27
        }

        public enum Covering
        {
            NONE,
            MOSS
        }

        public Tile()
        {
            hitbox = new Hitbox(new Vector2[] { new Vector2(0, 0), new Vector2(Tile.tilesize, 0), new Vector2(Tile.tilesize, -Tile.tilesize), new Vector2(0, -Tile.tilesize) }, Vector2.Zero, 2);
        }
        //Each tile is 8x8
        public int health;
        public Hitbox hitbox;
        public int x;
        public Covering covering;
        public int y;
        public (int chunkX, int chunkY) chunkPos;
        public int rotation;
        public TileType Type;


    }
}
