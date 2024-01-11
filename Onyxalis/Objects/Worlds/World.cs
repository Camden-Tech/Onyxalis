using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class World
    {
        public Chunk[][] chunks;
        public int time;
        public Weather weather;
        public int seed;
        public Random worldRandom;
        public static World CreateWorld()
        {
            World world = new World();
            world.GenerateSeed();
            world.weather = new Weather();
            return world;
        }


        public void GenerateSeed()
        {
            seed = Game1.GameRandom.Next();
            worldRandom = new Random(seed);
        }

        public Vector2 SpawnPlayerIn()
        {
            Vector2 position = new Vector2();
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    GenerateChunk(x, y);
                }
            }
            //Code for finding viable spawnpoint.
            return position;
        }
    }
}
