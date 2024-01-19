using Lucene.Net.Support;
using MiNET.Utils;
using Onyxalis.Objects.Entities;
using Onyxalis.Objects.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class PartialChunk
    {
        public Tile[,] tiles = new Tile[64, 64];

        public int x;
        public int y;
    }
}
