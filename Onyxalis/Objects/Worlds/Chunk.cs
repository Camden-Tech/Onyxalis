using Icaria.Engine.Procedural;
using Lucene.Net.Search;
using Lucene.Net.Support;
using Maybe.SkipList;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Effects;
using MiNET.Net;
using MiNET.Utils;
using Onyxalis.Objects.Entities;
using Onyxalis.Objects.Math;
using Onyxalis.Objects.Tiles;
using Onyxalis.Objects.Worlds;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    
    public class Chunk
    {
        public Tile[,] tiles = new Tile[64,64];

        public HashMap<UUID, LivingCreature> nonPlayers;
        
        public float[,] mossMap = new float[64,64];
        public float[] heightMap = new float[64];
        public float[,] caveMap = new float[64, 64];
        public int x;
        public int y;

        public Biome biome;
        public bool loaded;
        public World world; //Do not serialize
        public static float[] GenerateHeightMap(int x, int y, World world)
        {
            float[] perlinNoise = PerlinNoiseGenerator.GeneratePerlinNoise(64, 4, 0.25f, 1f, 1, world.seed, x * 64 - 1);
            float[] heightMap = new float[64];
            for (int i = 0; i < 64; i++)
            {
                heightMap[i] = (perlinNoise[i] / 1.33203125f) * 5;
            }
            for (int i = 0; i < 64; i++)
            {
                heightMap[i] += 32 - y * 64;
            }

            return heightMap;
        }




        public static float[,] GenerateMossMap(int x, int y, World world)
        {
            float[,] mossmap = new float[64, 64];
            for (int X = 0; X < 64; X++)
            {
                for (int Y = 0; Y < 64; Y++)
                {
                    mossmap[X,Y] = (PerlinNoiseGenerator.Generate2DPerlinNoise(x* 64 + X, y* 64 + Y, 4, 0.25f, 0.04f, 1f, world.seed)  / 1.33203125f) + 0.5f;
                }
            }

            return mossmap;
        }


        public Tile.Covering GenerateMoss(int X, int Y) {
            if(mossMap[X,Y] > 0.7f) {
                return Tile.Covering.MOSS;
            }
            return Tile.Covering.NONE;
        }

        public static bool canGenerateTile(int Y, float height, float caveNoise)
        {
            float adjustedNoiseGap = 0;
            if (Y <= height) {
                if (height + 1 - Y < 1) adjustedNoiseGap = 0.075f;
                else adjustedNoiseGap = 0.075f / (height + 1 - Y);
                if (!(caveNoise > 0.2 + adjustedNoiseGap) || !(caveNoise < 0.4 - adjustedNoiseGap))
                {
                    return true;
                }
            }
            return false;
        }

        private Tile GenerateTile(int X, int Y)
        {
            Tile tile = new Tile
            {
                x = X + x * 64,
                chunkPos = (X, Y),
                y = Y + y * 64,
                covering = Tile.Covering.NONE
            };

            tile.hitbox.Position = new Microsoft.Xna.Framework.Vector2(tile.x * Tile.tilesize, tile.y * Tile.tilesize);
            float height = heightMap[X];
            bool freezing = biome.temperature < 0;
            bool hot = biome.temperature > 90;
            bool mossHospitable = (biome.temperature > 20 && biome.temperature < 80);
            bool matchesHeight = Y == (int)height;
            bool belowHeightMap = height - Y <= 0;
            float noise = caveMap[X, Y];
            int adjustedY = y * 64 + Y;
            float adjustedNoiseGap = 0;
            float diff = height + y * 64 - adjustedY;
            if(diff > 120) adjustedNoiseGap = 0;
            else adjustedNoiseGap = 0.08f * (1 - diff / 120);

            if (!(noise > 0.4 + adjustedNoiseGap) || !(noise < 0.6 - adjustedNoiseGap))
            {
                if (!belowHeightMap)
                {
                    if (height <= 75)
                    {

                        if (matchesHeight)
                        {
                            if (!freezing && !hot)
                            {
                                tile.Type = Tile.TileType.GRASS;
                                tile.rotation = 0;
                                return tile;
                            }

                            if (freezing)
                            {
                                tile = GenerateSnow(tile);
                                return tile;
                            }
                            tile = GenerateSand(tile);
                            return tile;

                        }
                    }
                    if (Y < (int)height - 40)
                    {
                        if (Y < (int)height - 160)
                        {
                            tile = GenerateDeepRock(tile);
                            if (mossHospitable)
                            {
                                tile.covering = GenerateMoss(X, Y);
                            }
                            return tile;
                        }
                        tile = freezing ? GeneratePermafrost(tile) : GenerateStone(tile);
                        return tile;
                    }
                    int dif = (int)(height + y * 64);
                    if (height + y * 64 > 75)
                    {
                        if (matchesHeight)
                        {
                            tile = GenerateSnow(tile);
                            return tile;
                        }

                        if (world.worldRandom.NextDouble() / (dif - 75) < 0.05)
                        {
                            tile = GenerateStone(tile);
                            return tile;
                        }

                        tile = GenerateDirt(tile);
                        return tile;

                    }


                    tile = hot ? GenerateSand(tile) : GenerateDirt(tile);
                    return tile;
                }
            }
          return null;
        }


        public Tile.TileType getTileTypeBetweenTileTypes(Tile.TileType first, Tile.TileType last)
        {
            int rand = world.worldRandom.Next(last + 1 - first);
            return first + rand;
        }

        public Tile GenerateGrass(Tile tile)
        {
            if (world.worldRandom.Next(5) > 3)
            {
                tile.Type = getTileTypeBetweenTileTypes(Tile.TileType.SHORTGRASS, Tile.TileType.TALLGRASS);
                tile.rotation = 0;
                return tile;
            }
            return null;
            
            
        }


        public Tile GenerateDirt(Tile tile)
        {
            tile.Type = getTileTypeBetweenTileTypes(Tile.TileType.DIRT1, Tile.TileType.DIRT4);
            tile.rotation = world.worldRandom.Next(4);

            return tile;
        }
        public Tile GenerateSnow(Tile tile)
        {
            tile.Type = getTileTypeBetweenTileTypes(Tile.TileType.SNOW1, Tile.TileType.SNOW4);
            tile.rotation = world.worldRandom.Next(4);
            return tile;
        }
        public Tile GeneratePermafrost(Tile tile)
        {
            tile.Type = getTileTypeBetweenTileTypes(Tile.TileType.PERMAFROST1, Tile.TileType.PERMAFROST4);
            tile.rotation = world.worldRandom.Next(4);
            return tile;
        }
        public Tile GenerateSand(Tile tile)
        {
            tile.Type = getTileTypeBetweenTileTypes(Tile.TileType.SAND1, Tile.TileType.SAND4);
            tile.rotation = world.worldRandom.Next(4);
        
            return tile;
        }
        public Tile GenerateStone(Tile tile)
        {
            tile.Type = Tile.TileType.STONE;
            tile.rotation = world.worldRandom.Next(4);
            return tile;
        }
        public Tile GenerateDeepRock(Tile tile)
        {
            tile.Type = getTileTypeBetweenTileTypes(Tile.TileType.DEEPROCK1, Tile.TileType.DEEPROCK4);
            tile.rotation = world.worldRandom.Next(4);
            
            return tile;
        }

        public Tile GenerateFoliage(int X,int Y, float height){
            bool freezing = biome.temperature < 0;
            bool hot = biome.temperature > 90;
            
            
            
            Tile tile = new Tile
            {
                x = X + x * 64,
                chunkPos = (X, Y),
                y = Y + y * 64,
                covering = Tile.Covering.NONE
            };
            
            
            if (height < 75)
            {
                
                if (!freezing && !hot)
                {
                    tile = GenerateGrass(tile);
                    return tile;
                    
                }
                if (freezing && world.worldRandom.Next(11) >= 9)
                {
                    tile.Type = Tile.TileType.SHRUB;
                    tile.rotation = 0;
                    return tile;
                }
            }
            return null;
            
        }

        public void GenerateTiles()
        {
            for (int X = 0; X < 64; X++)
            {
                for (int Y = 0; Y < 64; Y++)
                {
                    Tile tile = GenerateTile(X, Y);
                    if(tile != null){
                        (Tile.DigType type, int health) = Tile.TileDictionary[tile.Type];
                        tile.health = health;
                        tile.digType = type;
                        tiles[X, Y] = tile;
                    }
                }
            }
            //Foliage
            int tooClose = 0;

            float[,] caveMap = GenerateCaveMap(x, y - 1, world);

            for (int X = 0; X < 64; X++){
                if(tooClose > 0){
                    tooClose--;
                    continue;
                }
                
                float height = heightMap[X];
                int Y = (int)height + 1;
                if (Y >= 0 && Y < 64) {
                    if (Y == 0)
                    {
                        if (!canGenerateTile(Y - 1, height - 1, caveMap[X, 64])) continue;
                    } else
                    {
                        if (tiles[X, Y - 1] == null) continue;
                    }
                    Tile tile = GenerateFoliage(X, Y, height);
                    if (tile != null) {
                        (Tile.DigType type, int health) = Tile.TileDictionary[tile.Type];
                        tile.health = health;
                        tile.digType = type;
                        tooClose = 5;
                        tiles[X, Y] = tile;
                    }
                }
            }

        }


        public static float[,] GenerateCaveMap(int x, int y, World world)
        {
            float[,] cavemap = new float[64, 64];
            for (int X = 0; X < 64; X++)
            {
                for (int Y = 0; Y < 64; Y++)
                {
                    cavemap[X, Y] = (PerlinNoiseGenerator.Generate2DPerlinNoise(x * 64 + X, y * 64 + Y, 4, 0.25f, 0.01f, 1f, world.seed) / 1.33203125f) + 0.5f;
                }
            }

            return cavemap;
        }


        public int GetMaxAmount(Biome.BiomeType biome){
            int amount = 0;
            if(biome == Biome.BiomeType.Underground){
                amount = 4;
            } else if (biome == Biome.BiomeType.Surface)
            {
                amount = 1;
            }
            return amount;
        }
        

        public static Chunk CreateChunk(int X, int Y, World world, bool GenerateTiles)
        {
            Chunk newChunk = new Chunk();
            newChunk.x = X;
            newChunk.y = Y;
            newChunk.world = world;
            newChunk.biome = world.getBiome(X, Y);
            newChunk.mossMap = GenerateMossMap(X, Y, world);
            newChunk.caveMap = GenerateCaveMap(X, Y, world);
            newChunk.heightMap = GenerateHeightMap(X, Y, world);
            newChunk.biome.type = Biome.GetBiomeType(newChunk.heightMap);
            int seed = world.seed;
           
            if(GenerateTiles) newChunk.GenerateTiles();
            return newChunk;
        }

    }
}
