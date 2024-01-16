using Icaria.Engine.Procedural;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Worlds;
using Newtonsoft.Json;
using Onyxalis.Objects.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    /*public class ChunkCluster //Not needed now that I know perlin noise is not recursive :) major refactoring time
    {
        public float[] heightMap = new float[1024]; // 1028 and 16 + 16 for lerping
        public float[] heightMapToLeft = new float[129];
        public float[] heightMapToRight = new float[129];
        public Chunk[,] chunks = new Chunk[16, 16];
        public Biome[] biomes = new Biome[16];
        public bool surface;
        public BiomeMap map;
        public World world;
        public int x;
        public int y;
        public Random chunkRandom;

        public ChunkCluster(World World)
        {

            world = World;
            chunkRandom = new Random(world.seed);
        }



        public Biome[] grabBiomes()
        {

            (int x, int y) insideMapIndex = World.findBiomeMapPosition(x, y);
            int index = x - (insideMapIndex.x * 8);
            for (int i = 0; i < 16; i++)
            {
                biomes[i] = map.biomes[index, i];
            }
            return biomes;
        }

        public float[] GenerateHeightMap()
        {
            ChunkCluster mapToLeft = world.clusters[x - 1, y];
            ChunkCluster mapToRight = world.clusters[x + 1, y];
            float[] uneditedPerlinNoise = GeneratePerlinNoiseWithBiomeInfluence(1280, 4, 0.25f, 1f, 50, world.seed, biomes, x * 1024);
            float[] perlinNoise = new float[1024];
            float[] unsplitPerlinNoise = new float[uneditedPerlinNoise.Length];
            int a = 0;
            foreach (float f in uneditedPerlinNoise)
            {
                unsplitPerlinNoise[a] = f + 512;
                a++;

            }
            Array.Copy(unsplitPerlinNoise, 128, perlinNoise, 0, 1024);
            Array.Copy(unsplitPerlinNoise, 1023, heightMapToRight, 0, 129);
            Array.Copy(unsplitPerlinNoise, 0, heightMapToLeft, 0, 129);
            for (int i = 0; i < 1024; i++)
            {
                if (i <= 128 && mapToLeft != null)
                {
                    int adjustedI = 128 - i;
                    heightMap[adjustedI] = ((mapToLeft.heightMapToRight[i] - perlinNoise[adjustedI]) * i / (128 + (128 - i) / 3)) + perlinNoise[adjustedI];
                }
                else if (i >= 896 && mapToRight != null)
                {
                    int adjustedI = i - 896;
                    heightMap[i] = ((mapToRight.heightMapToLeft[adjustedI] - perlinNoise[i]) * adjustedI / (128 + (128 - adjustedI) / 3)) + perlinNoise[i];
                }
                else
                {
                    heightMap[i] = perlinNoise[i];
                }

            }

           return heightMap;
        }
        public float[] GeneratePerlinNoiseWithBiomeInfluence(int width, int octaves, float persistence, float frequency, float amplitude, int seed, Biome[] biomes, int start)
        {
            float[] noiseMap = new float[width];

            for (int octave = 0; octave < octaves; octave++)
            {
                for (int x = 0; x < width; x++)
                {
                    int biomeI = (x * biomes.Length) / width;
                    Biome biome = biomes[biomeI];
                    (float amp, float freq) = Biome.biomeStats[(int)biome.type];
                    float xCoord = (x+start) * frequency / width;

                    float perlinValue = IcariaNoise.GradientNoise(xCoord, 0, seed);
                    noiseMap[x] += perlinValue  * amplitude;
                    
                    
                }


                frequency *= 2; // Increase the frequency for the next octave
                amplitude *= persistence; // Reduce the amplitude for the next octave
            }
            return noiseMap;
        }

        public Chunk GenerateChunk(int X, int Y, bool surfaceChunk)
        {
            Chunk newChunk = Chunk.CreateChunk(X, Y, world, true, surfaceChunk, this); //Create CreateChunk() method, the boolean decides whether the chunk generates terrain or not.
            
            chunks[X, Y] = newChunk;
            return newChunk;
        }

    }*/
}
