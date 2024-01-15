using MiNET.Worlds;
using Onyxalis.Objects.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class ChunkCluster
    {
        public float[] heightMap = new float[1092]; // 1028 and 16 + 16 for lerping
        public Chunk[,] chunks = new Chunk[16, 16];
        public Biome[] biomes = new Biome[16];
        public World world;
        public int x;
        public int y;
        public int seed;
        public Random chunkRandom;

        public ChunkCluster()
        {
            GenerateSeed(); 
            chunkRandom = new Random(seed);
        }

        public void GenerateSeed()
        {
            seed = Environment.TickCount + new Random(x).Next(10000) + new Random(y).Next(10000);
        }

        public float[] GenerateHeightMap()
        {
            
            ChunkCluster mapToLeft = world.clusters[x - 1, y];
            ChunkCluster mapToRight = world.clusters[x + 1, y];
            float[] perlinNoise = PerlinNoiseGenerator.GeneratePerlinNoise(1092, 4, 2, 1f, 3, seed);
            for (int i = 0; i < 1092; i++)
            {
                if (i <= 64 && mapToLeft != null)
                {
                    int adjustedI = 64 - i;
                    heightMap[adjustedI] = ((mapToLeft.heightMap[1091 - i] - perlinNoise[adjustedI]) * i / (128-i)) + perlinNoise[adjustedI];
                }
                else if (i > 995 && i <= 1027 && mapToRight != null)
                {
                    heightMap[i] = ((mapToRight.heightMap[0] - perlinNoise[i]) * (i - 995) / (64-i)) + perlinNoise[i];
                }
                else
                {
                    heightMap[i] = perlinNoise[i];
                }

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
