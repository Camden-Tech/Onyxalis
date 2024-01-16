using Icaria.Engine.Procedural;
using Lucene.Net.Search;
using Lucene.Net.Support;
using MiNET.Effects;
using MiNET.Net;
using MiNET.Utils;
using Onyxalis.Objects.Entities;
using Onyxalis.Objects.Math;
using Onyxalis.Objects.Tiles;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    
    public class Chunk
    {
        public Tile[,] tiles = new Tile[64,64];

        public HashMap<UUID, LivingCreature> nonPlayers;

        public bool surfaceChunk;


        public float[] heightMap = new float[64];

        public int x;
        public int y;

        public Biome biome;

        public bool loaded;
        public World world; //Do not serialize
        public float[] GenerateHeightMap()
        {
            float[] perlinNoise = PerlinNoiseGenerator.GeneratePerlinNoise(65, 4, 0.25f, 1f, 1, world.seed, x * 64 - 1);
            float[] map = new float[65];
            for (int i = 0; i < 65; i++)
            {
                map[i] = (perlinNoise[i] / 1.33203125f) + 32 + y * 64;
                
            }
            for (int i = 0; i < 64; i++)
            {
                float dif = map[i + 1] - map[i];
                (float amp, float freq) = Biome.biomeStats[(int)biome.type];
                heightMap[i] = map[i] + dif * biome.amplitude * amp;
            }

            return heightMap;
        }
        public void GenerateTiles()
        {
                for (int X = 0; X < 64; X++)
                {
                    float height = heightMap[X] - y * 64;
                    for (int Y = 0; Y < height && Y < 64; Y++)
                    {
                        Tile tile = new Tile();
                        tile.x = X + x * 64;
                        tile.chunkPos = (X, Y);
                        tile.y = Y + y * 64;
                        if (Y == (int)height && height <= 512) {
                            tile.Type = (Tile.TileType)world.worldRandom.Next(2);
                            tile.rotation = 0;
                        } else if (Y < (int)height - 40)
                        {
                            tile.Type = Tile.TileType.STONE;
                            tile.rotation = world.worldRandom.Next(4);

                        } else if (height > 512)
                        {
                            int heightDif = (int)height - 512;
                            if (world.worldRandom.NextDouble() / heightDif < 0.05)
                            {
                                tile.Type = Tile.TileType.STONE;
                                tile.rotation = world.worldRandom.Next(4);
                            }
                            else
                            {
                                tile.Type = (Tile.TileType)world.worldRandom.Next(4) + 2;
                                tile.rotation = world.worldRandom.Next(4);
                            }
                        }
                        else
                        {
                            tile.Type = (Tile.TileType)world.worldRandom.Next(4) + 2;
                            tile.rotation = world.worldRandom.Next(4);
                        }
                        tile.hitbox.Position = new Microsoft.Xna.Framework.Vector2(tile.x * Tile.tilesize, tile.y * Tile.tilesize);
                        tiles[X, Y] = tile;
                    }
                
            }
        }




        public static Chunk CreateChunk(int X, int Y, World world, bool GenerateTiles)
        {
            Chunk newChunk = new Chunk();

            newChunk.biome = world.getBiome(X,Y);
            newChunk.x = X;
            newChunk.y = Y;
            int seed = world.seed;
            newChunk.world = world;
            if(GenerateTiles) newChunk.GenerateTiles();
            return newChunk;
        }
        
    }
}
