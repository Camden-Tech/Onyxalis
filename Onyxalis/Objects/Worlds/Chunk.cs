using Onyxalis.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class Chunk
    {
        public Tile[][] tiles;

        public List<LivingCreature> nonPlayers;

        public int x;
        public int y;

    }
}
