using Lucene.Net.Support;
using MiNET.Utils;
using Onyxalis.Objects.Entities;
using Onyxalis.Objects.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class Chunk
    {
        public Tile[,] tiles = new Tile[64,64];

        public float[] heightMap = new float[64];

        public HashMap<UUID, LivingCreature> nonPlayers;

        public bool surfaceChunk;

        public int x;
        public int y;

        public World world;

        public float[] GenerateHeightMap()
        {
            float[,] perlinNoise = PerlinNoiseGenerator.GeneratePerlinNoise(64, 64, 4, 1);
            for (int i = 0; i < 64; i++)
            {
                heightMap[i] = (perlinNoise[0, i]) + 32;
            }
            return heightMap;
        }
        public void GenerateTiles()
        {
            GenerateHeightMap();
            for (int X = 0; X < 64; X++)
            {
                for (int Y = 0; Y < (surfaceChunk ? 64 : heightMap[X]); Y++)
                {
                    Tile tile = new Tile();
                    tile.x = X + x * 64;
                    tile.y = Y + x * 64;
                    tiles[X, Y] = tile; 
                }
            }
        }
        
        public static Chunk CreateChunk(World world, bool GenerateTiles, bool SurfaceChunk)
        {
            Chunk newChunk = new Chunk();
            int seed = world.seed;
            newChunk.surfaceChunk = SurfaceChunk;
            if(GenerateTiles) newChunk.GenerateTiles();
            return newChunk;
        }
        
    }
}
