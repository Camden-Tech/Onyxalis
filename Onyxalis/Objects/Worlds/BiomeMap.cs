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
        public float[] biomePerlinNoiseMap = new float[160]; //128 and 16 + 16 (lerp chunks)
        public float[] temperaturePerlinNoiseMap = new float[160]; //128 and 16 + 16 (lerp chunks)
        int seed;
        Random mapRandom;
        int x;
        int y;
        World world;
        public BiomeMap(World World)
        {
            GenerateSeed();
            mapRandom = new Random(seed);
            world = World;
        }


        public void GenerateSeed()
        {
            seed = Environment.TickCount + new Random(x).Next(100) + new Random(y).Next(100);
        }
        public float[] GenerateBiomeMap()
        {
            BiomeMap mapToLeft = world.biomeMaps[x - 1, y];
            BiomeMap mapToRight = world.biomeMaps[x + 1, y];
            float[] perlinNoise = PerlinNoiseGenerator.GeneratePerlinNoise(160, 4, 2, 5f, 4, seed);
            for (int i = 0; i < 160; i++)
            {
                if (i < 16 && mapToLeft != null)
                {
                    biomePerlinNoiseMap[i] = ((mapToLeft.biomePerlinNoiseMap[i + 144] - perlinNoise[i]) / 16) * i + perlinNoise[i];
                } else if(i > 144 && mapToRight != null)
                {
                    biomePerlinNoiseMap[i] = ((mapToRight.biomePerlinNoiseMap[i - 144] - perlinNoise[i]) / 16) * (i-144) + perlinNoise[i];
                } else
                {
                    biomePerlinNoiseMap[i] = perlinNoise[i];
                }
                
            }

            return biomePerlinNoiseMap;
        }
        public float[] GenerateTemperatureMap()
        {

            float[] perlinNoise = PerlinNoiseGenerator.GeneratePerlinNoise(160, 4, 2, 5f, 4, seed);
            for (int i = 0; i < 160; i++)
            {
                temperaturePerlinNoiseMap[i] = perlinNoise[i];
            }

            return temperaturePerlinNoiseMap;
        }
    }
}
