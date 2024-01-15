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
            GRASS,
            GRASS2,
            DIRT1,
            DIRT2,
            DIRT3,
            DIRT4,
            STONE,
            WOOD
        }
        public Tile()
        {
            hitbox = new Hitbox(new Vector2[] { new Vector2(0, 0), new Vector2(Tile.tilesize, 0), new Vector2(Tile.tilesize, Tile.tilesize), new Vector2(0, Tile.tilesize) }, Vector2.Zero);
        }
        //Each tile is 8x8
        public Hitbox hitbox;
        public int x;
        public int y;
        public (int chunkX, int chunkY) chunkPos;
        public int rotation;
        public TileType Type;


    }
}
