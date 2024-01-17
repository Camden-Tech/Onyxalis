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
        

        public float[] heightMap = new float[64];
        public (int x, int y, int radius)[] cavePoints;
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

        public static (int x, int y, int radius)[] GenerateCavePoints(int x, int y, World world, int maxAmount)
            {
            if (maxAmount > 0) {
                (int x, int y, int radius)[] cavePoints;
                Random cavePointRandom = new Random(world.seed + new Random(x).Next(4000) + new Random(y).Next(4000));
                int size = cavePointRandom.Next(maxAmount - 1) + 1;
                cavePoints = new (int x, int y, int radius)[size];
                for (int i = 0; i < size; i++) {
                    cavePoints[i] = (cavePointRandom.Next(64), cavePointRandom.Next(64), cavePointRandom.Next(15) + 5);
                }
                return cavePoints;
            } else
            {
                return new (int x, int y, int radius)[] { (0, 0, 0) };
            }
        }


        private Tile GenerateTile(int X, int Y)
        {
            Tile tile = new Tile
            {
                x = X + x * 64,
                chunkPos = (X, Y),
                y = Y + y * 64
                
        };
            tile.hitbox.Position = new Microsoft.Xna.Framework.Vector2(tile.x * Tile.tilesize, tile.y * Tile.tilesize);
            float height = heightMap[X];
            bool freezing = biome.temperature < 0;
            bool hot = biome.temperature > 90;
            bool matchesHeight = Y == (int)height;
            bool rightAboveHeight = Y == (int)height + 1;
            bool belowHeightMap = height - Y <= 0;
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
            } else
            {
                if (rightAboveHeight && height < 75)
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

        public void GenerateTiles()
        {
            
        
            for (int X = 0; X < 64; X++)
            {
                for (int Y = 0; Y < 64; Y++)
                {
                    tiles[X, Y] = GenerateTile(X, Y);
                }
            }
            for (int Xs = -1; Xs < 2; Xs++)
            {
                for (int Ys = -1; Ys < 2; Ys++)
                {
                    (int x, int y, int radius) oldPoint = (0,0,-1);
                    (int x, int y, int radius)[] points = GenerateCavePoints(Xs + x, Ys + y, world, GetMaxAmount(Biome.GetBiomeType(GenerateHeightMap(Xs + x, Ys + y, world))));
                    for (int i = 0; i < points.Length; i++)
                    {
                        (int x, int y, int radius) point = points[i];
                        if (oldPoint != (0,0,-1)) {
                            int lineX = oldPoint.x - point.x;
                            int lineY = oldPoint.y - point.y;
                            float lineDist = MathF.Sqrt(MathF.Pow(lineX, 2) + MathF.Pow(lineY, 2));
                            for (int lineI = 0; lineI < lineDist; lineI++) {
                                for (int x = -point.radius; x < point.radius; x++) //Circle
                                {
                                    int adjustedX = x + point.x + Xs * 64 + (int)(lineX * lineI / lineDist);

                                    if (adjustedX > -1 && adjustedX < 64)
                                    {
                                        for (int y = -point.radius; y < point.radius; y++)
                                        {
                                            float divident = lineI / lineDist;
                                            int adjustedY = y + point.y + Ys * 64 + (int)(lineY * divident) - (int)(5 * divident);
                                            if (adjustedY > -1 && adjustedY < 64)
                                            {
                                                float distance = MathF.Sqrt(MathF.Pow(x, 2) + MathF.Pow(y, 2));
                                                if (distance <= point.radius)
                                                {
                                                    tiles[adjustedX, adjustedY] = null;
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        oldPoint = point;
                    }
                }
                
            }
            
        }

        public int GetMaxAmount(Biome.BiomeType biome){
            int amount = 0;
            if(biome == Biome.BiomeType.Underground){
                amount = 7;
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
            newChunk.heightMap = GenerateHeightMap(X, Y, world);
            newChunk.biome.type = Biome.GetBiomeType(newChunk.heightMap);
            newChunk.cavePoints = GenerateCavePoints(X, Y, world, newChunk.GetMaxAmount(newChunk.biome.type));
            int seed = world.seed;
           
            if(GenerateTiles) newChunk.GenerateTiles();
            return newChunk;
        }
        
    }
}
