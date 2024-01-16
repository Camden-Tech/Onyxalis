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
        public (int x, int y)[] cavesPoints = (int x, int y)[];
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

        public (int x, int y)[] generateCavePoints)(int maxAmount){
            Random cavePointRandom = new Random(world.seed + new Random(x).next(4000) + new Random(y).next(4000));
            int size = cavePointRandom.next(maxAmount);
            (int x, int y)[] cavePoints = (int x, int y)[size];
            for(int i = 0; i < size; i++){
                cavePoints[i] = (cavePointRandom.next(64), cavePointRandom.next(64));
            }
            
        }
        
        
        private Tile GenerateTile(int X, int Y, float height)
        {
            Tile tile = new Tile
            {
                x = X + x * 64,
                chunkPos = (X, Y),
                y = Y + y * 64
            };
        
            bool freezing = biome.temperature < 0;
            bool hot = biome.temperature > 80;
            bool matchesHeight = Y == (int)height;
            bool rightAboveHeight = Y + 1 == (int)height;
        
            if (height <= 32)
            {
                if (rightAboveHeight)
                {
                    if (!freezing && !hot && world.worldRandom.Next(5) > 3)
                    {
                        tile.Type = (Tile.TileType)(world.worldRandom.Next(2) + 12);
                        tile.rotation = 0;
                    }
                    else if (freezing && world.worldRandom.Next(10) > 9)
                    {
                        tile.Type = Tile.TileType.SHRUB;
                        tile.rotation = 0;
                    }
                }
                else if (matchesHeight)
                {
                    if (!freezing && !hot)
                    {
                        tile.Type = (Tile.TileType)world.worldRandom.Next(2);
                        tile.rotation = 0;
                    }
                    else if (freezing)
                    {
                        tile.Type = Tile.TileType.SNOW;
                        tile.rotation = world.worldRandom.Next(4);
                    }
                    else if (hot)
                    {
                        tile.Type = Tile.TileType.SAND;
                        tile.rotation = world.worldRandom.Next(4);
                    }
                }
            }
            else if (Y < (int)height - 40)
            {
                if (Y < (int)height - 160)
                {
                    tile.Type = (Tile.TileType)(7 + world.worldRandom.Next(2));
                    tile.rotation = 0;
                }
                else
                {
                    tile.Type = freezing ? Tile.TileType .PERMAFROST : Tile.TileType.STONE;
                    tile.rotation = world.worldRandom.Next(4);
                }
            }
            else if (height > 32)
            {
                int heightDif = (int)height - 32;
                if (matchesHeight)
                {
                    tile.Type = Tile.TileType.SNOW;
                    tile.rotation = world.worldRandom.Next(4);
                }
                else if (world.worldRandom.NextDouble() / heightDif < 0.05)
                {
                    tile.Type = Tile.TileType.STONE;
                    tile.rotation = world.worldRandom.Next(4);
                }
                else
                {
                    tile.Type = (Tile.TileType)(world.worldRandom.Next(4) + 2);
                    tile.rotation = world.worldRandom.Next(4);
                }
            }
            else
            {
                tile.Type = hot ? Tile.TileType.SAND : (Tile.TileType)(world.worldRandom.Next(4) + 2);
                tile.rotation = world.worldRandom.Next(4);
            }
        
            tile.hitbox.Position = new Microsoft.Xna.Framework.Vector2(tile.x * Tile.tilesize, tile.y * Tile.tilesize);
            return tile;
        }
        public void GenerateTiles()
        {
            
        
            for (int X = 0; X < 64; X++)
            {
                float height = heightMap[X] - y * 64;
                for (int Y = 0; Y < height && Y < 64; Y++)
                {
                    tiles[X, Y] = GenerateTile(X, Y, height);
                }
            }
            
            
        }

        public void GenerateSubBiomes(){
            
        }
        

        public static Chunk CreateChunk(int X, int Y, World world, bool GenerateTiles)
        {
            Chunk newChunk = new Chunk();
            newChunk.x = X;
            newChunk.y = Y;
            newChunk.world = world;
            newChunk.biome = world.getBiome(X, Y);
            
            newChunk.GenerateHeightMap();
            newChunk.biome.type = Biome.getBiomeType(heightMap);
           
            int seed = world.seed;
           
            if(GenerateTiles) newChunk.GenerateTiles();
            return newChunk;
        }
        
    }
}
