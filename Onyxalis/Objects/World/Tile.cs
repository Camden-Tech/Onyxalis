using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.World
{
    public class Tile
    {
        public enum TileType
        {
            AIR,
            GRASS,
            DIRT,
            STONE,
            WOOD
        }
        public int x { get; set; }
        public int y { get; set; }
        public TileType Type { get; set; }
    }
}
