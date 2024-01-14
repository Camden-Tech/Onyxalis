using MiNET.Worlds;
using Onyxalis.Objects.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class ChunkCluster
    {
        public float[] heightMap = new float[1024];
        public Chunk[,] chunks = new Chunk[16, 16];
        public World world;
        public int x;
        public int y;


        public float[] GenerateHeightMap()
        {
            float[] perlinNoise = PerlinNoiseGenerator.GeneratePerlinNoise(1024, 4, 2, 0.5f, 3);
            for (int i = 0; i < 1024; i++)
            {
                heightMap[i] = perlinNoise[i]+128;
            }
                
            return heightMap;
        }

        public Chunk GenerateChunk(int X, int Y, bool surfaceChunk)
        {
            Chunk newChunk = Chunk.CreateChunk(X, Y, world, true, surfaceChunk, this); //Create CreateChunk() method, the boolean decides whether the chunk generates terrain or not.
            chunks[X, Y] = newChunk;
            return newChunk;
        }

    }
}
