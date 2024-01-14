using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class Tile
    {
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
    }
}
