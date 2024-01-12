using System;
namespace Onyxalis.Objects.Math
{
  class PerlinNoiseGenerator
  {
      static float[,] GeneratePerlinNoise(int width, int height, int octaves, float persistence, float frequency, float amplitude)
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
  
                      float perlinValue = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1;
                      noiseMap[x, y] += perlinValue * amplitude;
                  }
              }
  
              frequency *= 2; // Increase the frequency for the next octave
              amplitude *= persistence; // Reduce the amplitude for the next octave
          }
  
          return noiseMap;
      }
          static float[,] GeneratePerlinNoise(int width, int height, int octaves, float persistence)
      {
          float frequency = 1f;
          float amplitude = 1f;
          return GeneratePerlinNoise(width, height, octaves, persistence, frequency, amplitude);
      }
  }
}
