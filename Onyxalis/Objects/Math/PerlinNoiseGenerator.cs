using Microsoft.Xna.Framework;
using Icaria.Engine.Procedural;
using System;
using System.Reflection.Metadata.Ecma335;

namespace Onyxalis.Objects.Math
{
  class PerlinNoiseGenerator
  {
      public static float[,] Generate2DPerlinNoise(int width, int height, int octaves, float persistence, float frequency, float amplitude)
      {
          float[,] noiseMap = new float[width, height];

          for (int octave = 0; octave < octaves; octave++)
          {
              for (int y = 0; y < height; y++)
              {
                  for (int x = 0; x < width; x++)
                  {
                      float xCoord = x * frequency / width;
                      float yCoord = y * frequency / height;

                      float perlinValue = IcariaNoise.GradientNoise(xCoord, yCoord) * 2 - 1;
                      noiseMap[x, y] += perlinValue * amplitude;
                  }
              }
  
              frequency *= 2; // Increase the frequency for the next octave
              amplitude *= persistence; // Reduce the amplitude for the next octave
          }
  
          return noiseMap;
      }
        public static float[] GeneratePerlinNoise(int width, int octaves, float persistence, float frequency, float amplitude, int seed)
        {
            float[] noiseMap = new float[width];

            for (int octave = 0; octave < octaves; octave++)
            {
                for (int x = 0; x < width; x++)
                {
                    float xCoord = x * frequency / width;

                    float perlinValue = IcariaNoise.GradientNoise(xCoord, 0, seed);
                    noiseMap[x] += perlinValue * amplitude;
                }
                
                frequency *= 2; // Increase the frequency for the next octave
                amplitude *= persistence; // Reduce the amplitude for the next octave
            }

            return noiseMap;
        }
        public static float[] GeneratePerlinNoise(int width, int octaves, float persistence, float frequency, float amplitude, int seed, int start)
        {
            float[] noiseMap = new float[width];

            for (int octave = 0; octave < octaves; octave++)
            {
                for (int x = start; x < width+start; x++)
                {
                    float xCoord = x * frequency / width;

                    float perlinValue = IcariaNoise.GradientNoise(xCoord, 0, seed);
                    noiseMap[x] += perlinValue * amplitude;
                }

                frequency *= 2; // Increase the frequency for the next octave
                amplitude *= persistence; // Reduce the amplitude for the next octave
            }

            return noiseMap;
        }
        public static float[] GeneratePerlinNoise(int width, int octaves, float persistence, int seed)
        {
            float frequency = 1f;
            float amplitude = 1f;
            return GeneratePerlinNoise(width, octaves, persistence, frequency, amplitude, seed);
        }
        public  static float[,] Generate2DPerlinNoise(int width, int height, int octaves, float persistence)
      {
          float frequency = 1f;
          float amplitude = 1f;
          return Generate2DPerlinNoise(width, height, octaves, persistence, frequency, amplitude);
      }
  }
}
