using MiNET.Effects;
using Onyxalis.Objects.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class BiomeMap
    {
        public float[] amplitudePerlinNoiseMap = new float[128];
        public float[] amplitudePerlinNoiseMapLeft = new float[33];
        public float[] amplitudePerlinNoiseMapRight = new float[33];
        public float[] biomePerlinNoiseMap = new float[128];
        public float[] biomePerlinNoiseMapLeft = new float[33];
        public float[] biomePerlinNoiseMapRight = new float[33];
        public float[] temperaturePerlinNoiseMap = new float[128];
        public float[] temperaturePerlinNoiseMapLeft = new float[33];
        public float[] temperaturePerlinNoiseMapRight = new float[33];//128 and 16 + 16 (lerp chunks)


        public Biome[,] biomes = new Biome[8,16];


        public int seed;
        public Random mapRandom;
        public int x;
        public int y;
        public World world;
        public BiomeMap(World World, int X, int Y)
        {
            x = X;
            y = Y;
            GenerateSeed();
            mapRandom = new Random(seed);
            world = World;
            GenerateAmplitudeMap();
            GenerateTemperatureMap();
            GenerateBiomeMap();
            GenerateBiomes();
        }



        public Biome[,] GenerateBiomes()
        {
            for (int clusterI = 0; clusterI < 8; clusterI++)
            {
                for (int chunkI = 0; chunkI < 16; chunkI++)
                {
                    int exactChunk = (clusterI * 16) + chunkI;
                    Biome biome = new Biome(Biome.GetTerrainType(biomePerlinNoiseMap[exactChunk]), temperaturePerlinNoiseMap[exactChunk], amplitudePerlinNoiseMap[exactChunk]);
                    biomes[clusterI, chunkI] = biome;

                }
            }
            return biomes;
        }



        public void GenerateSeed()
        {
            seed = Environment.TickCount + new Random(x).Next(100) + new Random(y).Next(100);
        }
        public float[] GenerateBiomeMap()
        {

            BiomeMap mapToLeft = world.biomeMaps[x - 1, y];
            BiomeMap mapToRight = world.biomeMaps[x + 1, y];
            float[] uneditedPerlinNoise = PerlinNoiseGenerator.GeneratePerlinNoise(160, 4, 1, 4, 1, seed);
            float[] perlinNoise = new float[128];
            float[] unsplitPerlinNoise = new float[uneditedPerlinNoise.Length];
            int a = 0;
            foreach (float f in uneditedPerlinNoise)
            {
                unsplitPerlinNoise[a] = (f / 1.33203125f)/2 + 0.5f;
                a++;

            }
            Array.Copy(unsplitPerlinNoise, 32, perlinNoise, 0, 128);
            Array.Copy(unsplitPerlinNoise, 127, biomePerlinNoiseMapRight, 0, 33);
            Array.Copy(unsplitPerlinNoise, 0, biomePerlinNoiseMapLeft, 0, 33);
            for (int i = 0; i < 128; i++)
            {
                if (i <= 32 && mapToLeft != null)
                {
                    int adjustedI = 32 - i;
                    biomePerlinNoiseMap[adjustedI] = ((mapToLeft.biomePerlinNoiseMapRight[i] - perlinNoise[adjustedI]) * i / (32 + (32 - i) / 3)) + perlinNoise[adjustedI];
                }
                else if (i >= 96 && mapToRight != null)
                {
                    int adjustedI = i - 96;
                    biomePerlinNoiseMap[i] = ((mapToRight.biomePerlinNoiseMapLeft[adjustedI] - perlinNoise[i]) * adjustedI / (32 + (32 - adjustedI) / 3)) + perlinNoise[i];
                }
                else
                {
                    biomePerlinNoiseMap[i] = perlinNoise[i];
                }

            }

            return biomePerlinNoiseMap;
        }

        public float[] GenerateAmplitudeMap()
        {

            BiomeMap mapToLeft = world.biomeMaps[x - 1, y];
            BiomeMap mapToRight = world.biomeMaps[x + 1, y];
            float[] uneditedPerlinNoise = PerlinNoiseGenerator.GeneratePerlinNoise(161, 4, 0.5f, 0.5f, 4, seed);
            float[] unsplitPerlinNoise = new float[uneditedPerlinNoise.Length];
            int a = 0;
            foreach (float f in uneditedPerlinNoise) {
                unsplitPerlinNoise[a] = f + 5;
                a++;
            
            }

            float[] perlinNoise = new float[128];
            Array.Copy(unsplitPerlinNoise, 32, perlinNoise, 0, 128);
            Array.Copy(unsplitPerlinNoise, 127, amplitudePerlinNoiseMapRight, 0, 33);
            Array.Copy(unsplitPerlinNoise, 0, amplitudePerlinNoiseMapLeft, 0, 33);
            for (int i = 0; i < 128; i++)
            {
                if (i <= 32 && mapToLeft != null)
                {
                    int adjustedI = 32 - i;
                    amplitudePerlinNoiseMap[adjustedI] = ((mapToLeft.amplitudePerlinNoiseMapRight[i] - perlinNoise[adjustedI]) * i / (32 + (32 - i) / 3)) + perlinNoise[adjustedI];
                }
                else if (i >= 96 && mapToRight != null)
                {
                    int adjustedI = i - 96;
                    amplitudePerlinNoiseMap[i] = ((mapToRight.amplitudePerlinNoiseMapLeft[adjustedI] - perlinNoise[i]) * adjustedI / (32 + (32 - adjustedI) / 3)) + perlinNoise[i];
                }
                else
                {
                    amplitudePerlinNoiseMap[i] = perlinNoise[i];
                }

            }

            return amplitudePerlinNoiseMap;
        }


        public float[] GenerateTemperatureMap()
        {

            BiomeMap mapToLeft = world.biomeMaps[x - 1, y];
            BiomeMap mapToRight = world.biomeMaps[x + 1, y];
            float[] unsplitPerlinNoise = PerlinNoiseGenerator.GeneratePerlinNoise(160, 4, 2, 3f, 4, seed);
            float[] perlinNoise = new float[128];
            Array.Copy(unsplitPerlinNoise, 32, perlinNoise, 0, 128);
            Array.Copy(unsplitPerlinNoise, 127, temperaturePerlinNoiseMapRight, 0, 33);
            Array.Copy(unsplitPerlinNoise, 0, temperaturePerlinNoiseMapLeft, 0, 33);
            for (int i = 0; i < 128; i++)
            {
                if (i <= 32 && mapToLeft != null)
                {
                    int adjustedI = 32 - i;
                    temperaturePerlinNoiseMap[adjustedI] = ((mapToLeft.temperaturePerlinNoiseMapRight[i] - perlinNoise[adjustedI]) * i / (32 + (32 - i) / 3)) + perlinNoise[adjustedI];
                }
                else if (i >= 96 && mapToRight != null)
                {
                    int adjustedI = i - 96;
                    temperaturePerlinNoiseMap[i] = ((mapToRight.temperaturePerlinNoiseMapLeft[adjustedI] - perlinNoise[i]) * adjustedI / (32 + (32 - adjustedI) / 3)) + perlinNoise[i];
                }
                else
                {
                    temperaturePerlinNoiseMap[i] = perlinNoise[i];
                }

            }

            return temperaturePerlinNoiseMap;
        }
    }
}
