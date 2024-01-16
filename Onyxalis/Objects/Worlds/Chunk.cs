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
                map[i] = (perlinNoise[i] / 1.33203125f);
            }
            for (int i = 0; i < 64; i++)
            {
                float dif = map[i + 1] - map[i];
                (float amp, float freq) = Biome.biomeStats[(int)biome.type];
                map[i + 1] = map[i] + dif * biome.amplitude * amp;
                heightMap[i] = map[i + 1];
            }
            for (int i = 0; i < 64; i++)
            {
                heightMap[i] += 32 - y * 64;
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
                    bool freezing = biome.temperature < 0;
                    bool hot = biome.temperature > 80;
                    bool matchesHeight = Y == (int)height;
                    bool rightAboveHeight = Y + 1 == (int)height;
                    if (height <= 32)
                    {
                        if (rightAboveHeight)
                        {
                            if (!freezing && !hot)
                            {
                                if (world.worldRandom.Next(5) > 3)
                                {
                                    tile.Type = (Tile.TileType)world.worldRandom.Next(2) + 12;
                                    tile.rotation = 0;
                                }
                            } else if (freezing)
                            {
                                if (world.worldRandom.Next(10) > 9)
                                {
                                    tile.Type = Tile.TileType.SHRUB;
                                    tile.rotation = 0;
                                }
                            }
                        } else if (matchesHeight)
                        { //If tile is the highest tile up, and the height is less than 33

                            if (!freezing && !hot)
                            { //Not too cold or hot, then generate grass
                                tile.Type = (Tile.TileType)world.worldRandom.Next(2);
                                tile.rotation = 0;
                            }
                            else if (freezing) //Too cold, generate snow
                            {
                                tile.Type = Tile.TileType.SNOW;
                                tile.rotation = world.worldRandom.Next(4);
                            }
                            else if (hot)
                            {
                                tile.Type = Tile.TileType.SAND; //Too hot, generate sand
                                tile.rotation = world.worldRandom.Next(4);
                            }
                        }
                    } else if (Y < (int)height - 40) { //If tile is 40 tiles down from farthest tile up

                        if (Y < (int)height - 160) //If tile is 160 tiles down from farthest tile up
                        {
                            tile.Type = (Tile.TileType)(7+world.worldRandom.Next(2)); //Generate deeprock
                            tile.rotation = 0;
                        } else
                        {
                            if (freezing) {
                                tile.Type = Tile.TileType.PERMAFROST; //Generate stone
                                tile.rotation = world.worldRandom.Next(4);
                            } else
                            {
                                tile.Type = Tile.TileType.STONE; //Generate stone
                                tile.rotation = world.worldRandom.Next(4);
                            }
                        }
                            

                    } else if (height > 32) //If height is above 32
                    {
                        int heightDif = (int)height - 32;
                        if (matchesHeight)
                        {
                            tile.Type = Tile.TileType.SNOW; //Too high, generate snow
                            tile.rotation = world.worldRandom.Next(4);
                        } else if (world.worldRandom.NextDouble() / heightDif < 0.05)
                        {
                            tile.Type = Tile.TileType.STONE; //Gradual increasing chance to generate snow
                            tile.rotation = world.worldRandom.Next(4);
                        }
                        else
                        {
                            tile.Type = (Tile.TileType)world.worldRandom.Next(4) + 2; //Generate dirt
                            tile.rotation = world.worldRandom.Next(4);
                        }
                    }
                    else  //Generates dirt/sand tiles
                    {
                        if (hot)
                        {
                            tile.Type = Tile.TileType.SAND; //Too hot, generate sand
                            tile.rotation = world.worldRandom.Next(4);
                        }
                        else
                        {
                            tile.Type = (Tile.TileType)world.worldRandom.Next(4) + 2; //just right, generate dirt
                            tile.rotation = world.worldRandom.Next(4);
                        }
                    }
                    tile.hitbox.Position = new Microsoft.Xna.Framework.Vector2(tile.x * Tile.tilesize, tile.y * Tile.tilesize);
                    tiles[X, Y] = tile;
                }
                
            }
        }




        public static Chunk CreateChunk(int X, int Y, World world, bool GenerateTiles)
        {
            Chunk newChunk = new Chunk();
            newChunk.world = world;
            newChunk.biome = world.getBiome(X, Y);
            newChunk.GenerateHeightMap();
            newChunk.x = X;
            newChunk.y = Y;
            int seed = world.seed;
           
            if(GenerateTiles) newChunk.GenerateTiles();
            return newChunk;
        }
        
    }
}
