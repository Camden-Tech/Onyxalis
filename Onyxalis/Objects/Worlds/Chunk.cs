using Lucene.Net.Support;
using MiNET.Utils;
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

        public HashMap<UUID, LivingCreature> nonPlayers;

        public int x;
        public int y;

        public World world;

        public void GenerateTiles()
        {
            
        }
        
        public static Chunk CreateChunk(World world, bool generateTiles)
        {
            Chunk newChunk = new Chunk();
            int seed = world.seed;
            if(generateTiles) newChunk.GenerateTiles();
            return newChunk;
        }
        
    }
}
