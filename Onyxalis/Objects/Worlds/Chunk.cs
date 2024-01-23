
using Icaria.Engine.Procedural;
using Lucene.Net.Search;
using Lucene.Net.Support;
using Maybe.SkipList;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Effects;
using MiNET.Entities.Hostile;
using MiNET.Items;
using MiNET.Net;
using MiNET.Utils;
using Onyxalis.Objects.Entities;
using Onyxalis.Objects.Math;
using Onyxalis.Objects.Tiles;
using Onyxalis.Objects.Worlds;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections;
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

        public Tile[,] tiles = new Tile[64, 64];
        public Background[,] backgrounds = new Background[64, 64];

        public HashMap<UUID, LivingCreature> nonPlayers;
        
        public float[,] mossMap = new float[64,64];
        public float[] heightMap = new float[64];
        public float[,] caveMap = new float[64, 64];
        public int[] forestPoints = new int[64];
        public int x;
        public int y;
        private Biome biomeToRight;
        private Biome biomeToLeft;
        private Biome biomeToBelow;
        private Biome biomeToAbove;
        public bool generatedTrees;
        public Biome biome;
        public bool loaded;
        public World world; //Do not serialize

        private enum blendCondition
        {
            tooCold,
            tooHot
        }

        public static float[,] GenerateCaveMap(int x, int y, World world)
        {
            const int size = 64;
            float[,] cavemap = new float[size, size];

            Parallel.For(0, size, X =>
            {
                int adjustedX = x * size + X;
                for (int Y = 0; Y < size; Y++)
                {
                    int adjustedY = y * size + Y;
                    cavemap[X, Y] = CalculateCaveValue(adjustedX, adjustedY, world);
                }
            });

            return cavemap;
        }

        private static float CalculateCaveValue(int x, int y, World world)
        {
            return (PerlinNoiseGenerator.Generate2DPerlinNoise(x, y, 4, 0.25f, 0.01f, 1f, world.seed) / 1.33203125f) + 0.5f;
        }
        public static int[] GenerateForestPoints(int x, int y, World world, Biome biome)
        {
            int[] forestPoints = new int[64];
            int i = 0;
            if (biome.temperature > 70) return forestPoints;
            for (int X = 0; X < 64; X++)
            {
                if (PerlinNoiseGenerator.GeneratePerlinNoise(4, 0.25f, 0.02f, 1f, world.seed, x * 64 + X) / 1.33203125f + 0.5f > (0.65f - (biome.horizontalType == Biome.HorizontalBiomeType.Forest ? 0.35f : 0)))
                {
                    forestPoints[i] = X;
                    i++;
                }
            }
            return forestPoints;
        }


        public static float[] GenerateHeightMap(int x, int y, World world, Biome biome)
        {
            const int size = 64;
            float[] perlinNoise = PerlinNoiseGenerator.GeneratePerlinNoise(size, 4, 0.25f, 0.125f, biome.amplitude * 4, world.seed, x * size);
            float[] heightMap = new float[size];

            for (int i = 0; i < size; i++)
            {
                heightMap[i] = CalculateHeightValue(perlinNoise[i], y, size);
            }

            return heightMap;
        }

        private static float CalculateHeightValue(float perlinValue, int y, int size)
        {
            return (perlinValue / 1.33203125f) * 5 + 32 - y * size;
        }


        public static float[,] GenerateMossMap(int x, int y, World world)
        {
            const int size = 64;
            float[,] mossmap = new float[size, size];

            Parallel.For(0, size, X =>
            {
                int adjustedX = x * size + X;
                for (int Y = 0; Y < size; Y++)
                {
                    int adjustedY = y * size + Y;
                    mossmap[X, Y] = CalculateMossValue(adjustedX, adjustedY, world);
                }
            });
            return mossmap;
        }

        private static float CalculateMossValue(int x, int y, World world)
        {
            return (PerlinNoiseGenerator.Generate2DPerlinNoise(x, y, 4, 0.25f, 0.04f, 1f, world.seed) / 1.33203125f) + 0.5f;
        }


        public Tile.Covering GenerateMoss(int X, int Y) {
            if(mossMap[X,Y] > 0.7f) {
                return Tile.Covering.MOSS;
            }
            return Tile.Covering.NONE;
        }

        public static bool canGenerateTile(int Y, float height, float caveNoise)
        {
          

            float diff = height + 1 - Y;
            if (diff > 120) return caveNoise <= 0.6f && caveNoise >= 0.4f;

            float adjustedNoiseGap = 0.09f * (1 - diff / 120);
            return !(caveNoise > 0.4 + adjustedNoiseGap && caveNoise < 0.6 - adjustedNoiseGap);
        }

        public Tile createTile(int x, int y, (int x, int y) chunkPos, int rotation, Tile.TileType type)
        {
            int adjX = x + this.x * 64;
            int adjY = y + this.y * 64;
            Tile tile = new Tile
            {
                x = adjX,
                chunkPos = chunkPos,
                y = adjY,
                rotation = setRotation(rotation),
                Type = type,
                covering = Tile.Covering.NONE
            };
            (Tile.DigType digType, int health) = Tile.TileDictionary[tile.Type];
            tile.health = health;
            tile.digType = digType;
            tile.hitbox.Position = new Microsoft.Xna.Framework.Vector2(tile.x * Tile.tilesize, tile.y * Tile.tilesize);
            
            
            return tile;
        }

        public Tile[,] createMultiTile(int x, int y, (int x, int y) chunkPos, int rotation, Tile.TileType type)
        {
            
            Texture2D[,] parts;
            try
            {
                parts = Game1.multiTilePieces[type];
            }
            catch (Exception e)
            { //die
                //throw exception
                return null;
            }
            Tile[,] tiles = new Tile[parts.GetLength(0), parts.GetLength(1)];
            for (int X = 0; X < parts.GetLength(0); X++)
            {
                for (int Y = 0; Y < parts.GetLength(1); Y++)
                {
                    int adjX = x + this.x * 64 + X;
                    int adjY = y + this.y * 64 + parts.GetLength(1) - Y - 1;
                    Tile tile = new Tile
                    {
                        x = adjX,
                        chunkPos = chunkPos,
                        y = adjY,
                        rotation = setRotation(rotation),
                        Type = type,
                        multiTile = true,
                        originalPos = (adjX, adjY),
                        piecePos = (X, Y),
                        covering = Tile.Covering.NONE
                    };
                    (Tile.DigType digType, int health) = Tile.TileDictionary[tile.Type];
                    tile.health = health;
                    
                    tile.digType = digType;
                    tile.hitbox.Position = new Microsoft.Xna.Framework.Vector2(tile.x * Tile.tilesize, tile.y * Tile.tilesize);
                    tiles[X,Y] = tile;
                }
            }
            return tiles;
        }


        private bool tooHot(Biome biome)
        {
            return biome.temperature + biome.heightTemp > 80;
        }
        private bool tooCold(Biome biome)
        {
            return biome.temperature + biome.heightTemp < 0;
        }

        private (bool, Tile) GenerateTile(int X, int Y, float height, int intHeight)
        {

            bool background = false;
            if (Y > height) return (false, null);
            if (!canGenerateTile(Y, height, caveMap[X, Y])) background = true;

            bool freezing = tooCold(biome);
            bool hot = tooHot(biome);
            

            bool matchesHeight = Y == intHeight;
            if (height >= -75f && matchesHeight)
            {
                return (background, GenerateSurfaceTile(X, Y, freezing, hot));
            }
            else if (Y < intHeight - 40)
            {
                return (background, GenerateSubSurfaceTile(X, Y, intHeight, freezing));
            }
            else
            {
                return (background, hot ? GenerateSand(X, Y) : GenerateDirt(X, Y));
            }
        }

        private Tile GenerateSurfaceTile(int X, int Y, bool freezing, bool hot)
        {
            if (!freezing && !hot) return createTile(X, Y, (X, Y), 0, getTileTypeBetweenTileTypes(Tile.TileType.GRASS, Tile.TileType.GRASS2));
            return freezing ? GenerateSnow(X, Y) : GenerateSurfaceSand(X, Y);
        }

        private Tile GenerateSubSurfaceTile(int X, int Y, int intHeight, bool freezing)
        {
            if (Y < intHeight - 160)
            {
                Tile tile = GenerateDeepRock(X, Y);
                if (IsMossHospitable()) tile.covering = GenerateMoss(X, Y);
                return tile;
            }
            return freezing ? GeneratePermafrost(X, Y) : GenerateStone(X, Y);
        }

        private bool IsMossHospitable()
        {
            int newTemp = (int)(biome.heightTemp + biome.temperature);
            return newTemp > 20 && newTemp < 80;
        }

        public Tile.TileType getTileTypeBetweenTileTypes(Tile.TileType first, Tile.TileType last)
        {
            int rand = Game1.GameRandom.Next(last + 1 - first);
            return first + rand;
        }

        public Tile[,] GenerateGrass(int X, int Y)
        {
            if (Game1.GameRandom.Next(5) > 2 + (MathF.Abs(biome.temperature - 60)) / 20)
            {
                Tile.TileType type = getTileTypeBetweenTileTypes(Tile.TileType.SHORTGRASS, Tile.TileType.LONGGRASS);
                int rotation = 0;
                Tile[,] tiles = createMultiTile(X, Y, (x, y), rotation, type);
                return tiles;
            }
            return null;
            
            
        }
        private Tile GenerateTileOfType(Tile.TileType firstType, Tile.TileType lastType, int X, int Y)
        {
            Tile.TileType type = GetRandomTileType(firstType, lastType);
            int rotation = Game1.GameRandom.Next(4);
            return createTile(X, Y, (X, Y), rotation, type);
        }

        private Tile.TileType GetRandomTileType(Tile.TileType first, Tile.TileType last)
        {
            return first + Game1.GameRandom.Next(last + 1 - first);
        }

        public Tile GenerateDirt(int X, int Y)
        {
            return GenerateTileOfType(Tile.TileType.DIRT1, Tile.TileType.DIRT4, X, Y);
        }

        public Tile GenerateSnow(int X, int Y)
        {
            return blendTile(X, Y, getTileTypeBetweenTileTypes(Tile.TileType.SNOW1, Tile.TileType.SNOW4), Game1.GameRandom.Next(4), getTileTypeBetweenTileTypes(Tile.TileType.GRASS, Tile.TileType.GRASS2), 0, blendCondition.tooCold);
        }

        public Tile GeneratePermafrost(int X, int Y)
        {
            return blendTile(X, Y, getTileTypeBetweenTileTypes(Tile.TileType.PERMAFROST1, Tile.TileType.PERMAFROST4), Game1.GameRandom.Next(4), Tile.TileType.STONE, Game1.GameRandom.Next(4), blendCondition.tooCold);
        }


        private Tile blendTile(int X, int Y, Tile.TileType type, int typeRot, Tile.TileType replacement, int replRot, blendCondition condition) //No, do not make that a delegate cause that could be computationally expensive, idk.
        {
            bool below = true;
            bool above = true;
            bool right = true;
            bool left = true;
            switch (condition) {
                case blendCondition.tooCold:
                    left = tooCold(biomeToLeft);
                    right = tooCold(biomeToRight);
                    above = tooCold(biomeToAbove);
                    below = tooCold(biomeToBelow);
                    break;
                case blendCondition.tooHot:
                    left = tooHot(biomeToLeft);
                    right = tooHot(biomeToRight);
                    above = tooHot(biomeToAbove);
                    below = tooHot(biomeToBelow);
                    break;
            }
            Tile tile = null;
            if (right && left && above && below) return createTile(X, Y, (X, Y), typeRot, type);
            else if (!right && X > 32) tile = Game1.GameRandom.NextDouble() * 6 / MathF.Pow(64 - X, 1.5f) > 0.03125f ? createTile(X, Y, (X, Y), replRot, replacement) : createTile(X, Y, (X, Y), typeRot, type);
            else if (!left && X < 32) tile = Game1.GameRandom.NextDouble() * 6 / MathF.Pow(X, 1.5f) > 0.03125f ? createTile(X, Y, (X, Y), replRot, replacement) : createTile(X, Y, (X, Y), typeRot, type);
            if (!above && Y > 32) tile = Game1.GameRandom.NextDouble() * 6 / MathF.Pow(64 - Y, 1.5f) > 0.03125f ? createTile(X, Y, (X, Y), replRot, replacement) : createTile(X, Y, (X, Y), typeRot, type);
            else if (!below && Y < 32) tile = Game1.GameRandom.NextDouble() * 6 / MathF.Pow(Y, 1.5f) > 0.03125f ? createTile(X, Y, (X, Y), replRot, replacement) : createTile(X, Y, (X, Y), typeRot, type);
            if (tile == null) return createTile(X, Y, (X, Y), typeRot, type);
            return tile;
        }


        public Tile GenerateSurfaceSand(int X, int Y)
        {

            return blendTile(X, Y, getTileTypeBetweenTileTypes(Tile.TileType.SAND1, Tile.TileType.SAND4), Game1.GameRandom.Next(4), getTileTypeBetweenTileTypes(Tile.TileType.GRASS, Tile.TileType.GRASS2), 0, blendCondition.tooHot);
        }
        public Tile GenerateSand(int X, int Y)
        {

            return blendTile(X, Y, getTileTypeBetweenTileTypes(Tile.TileType.SAND1, Tile.TileType.SAND4), Game1.GameRandom.Next(4), getTileTypeBetweenTileTypes(Tile.TileType.DIRT1, Tile.TileType.DIRT4), 0, blendCondition.tooHot);
        }
        public Tile GenerateStone(int X, int Y)
        {
            // Since STONE doesn't have a range, we directly call createTile
            int rotation = Game1.GameRandom.Next(4);
            return createTile(X, Y, (X, Y), rotation, Tile.TileType.STONE);
        }
        public Tile GenerateDeepRock(int X, int Y)
        {
            return GenerateTileOfType(Tile.TileType.DEEPROCK1, Tile.TileType.DEEPROCK4, X, Y);
        }

        public Tile[,] GenerateFoliage(int X,int Y, float height){
            bool freezing = biome.temperature < 0;
            bool hot = biome.temperature > 80;
            Tile[,] tiles;
            if (height < 75f)
            {
                if (!freezing && !hot)
                {
                    tiles = GenerateGrass(X,Y);
                    return tiles;
                }
                if (freezing && Game1.GameRandom.Next(11) >= 9)
                {
                    tiles = createMultiTile(X, Y, (x, y), 0, Tile.TileType.SHRUB);
                    return tiles;
                }
            }
            return null;
        }

        public List<Tile[,]> GenerateTree(int X, int Y)
        {
            int height = world.worldRandom.Next(6) + 5;
            List<Tile[,]> tileArray = new List<Tile[,]>();
            for (int i = 0; i <= height; i++)
            {
                Tile[,] tiles;
                if (i == 0)
                {
                    tiles = createMultiTile(X, Y + i * 4, (this.x, this.y), 0, Tile.TileType.TREESTUMP);
                    foreach (Tile tile1 in tiles)
                    {
                        if (world.tiles[tile1.x, tile1.originalPos.y - tiles.GetLength(1)] == null) return null;
                    }
                }
                else if (i < height) tiles = createMultiTile(X, Y + i * 4, (this.x, this.y), 0, Tile.TileType.TREESTALK);
                else if (i == height) tiles = createMultiTile(X - 2, Y + i * 4, (this.x, this.y), 0, getTileTypeBetweenTileTypes(Tile.TileType.TREETOP, Tile.TileType.TREETOP2));
                else tiles = null;
                if (tiles != null)
                {
                    tileArray.Add(tiles);

                }
            }
            return tileArray;
        }


        public void GenerateTiles()
        {
            Parallel.For(0, 64, X =>
            {
                float height = heightMap[X];
                int intHeight = (int)height;
                for (int Y = 0; Y < 64; Y++)
                {
                    (bool backdrop, Tile tile) = GenerateTile(X, Y, height, intHeight);
                    if (tile != null)
                    {
                        if (backdrop)
                        {
                            Background background = new Background
                            {
                                x = tile.x,
                                y = tile.y,
                                type = tile.Type
                            };
                            backgrounds[X, Y] = background;
                        } else tiles[X, Y] = tile;


                    }
                        
                }
                
            });
        }

        public void adjacentChunkLoaded()
        {
            if (generatedTrees)
            {
                return;
            }
            else
            {
                for (int X = -1; X < 2; X++)
                {
                    for (int Y = -1; Y < 2; Y++)
                    {
                        Chunk c = world.loadedChunks[(X + x, Y + y)];
                        if (c == null)
                        {
                            return;
                        }
                    }
                }
            }
            GenerateTrees();
            GeneratePlants();
            generatedTrees = true;


        }

        public void GeneratePlants()
        {
            int tooClose = 0;
            for (int X = 0; X < 64; X++)
            { //Foliage
                if (tooClose > 0)
                {
                    tooClose--;
                    continue;
                }

                float height = heightMap[X];
                int Y = (int)height + 1;
                if (Y >= 0 && Y < 64)
                {

                    Tile[,] tileArray = GenerateFoliage(X, Y, height);
                    bool goBack = false;
                    if (tileArray != null)
                    {
                        int l = tileArray.GetLength(1);
                        foreach (Tile tile1 in tileArray)
                        {
                            if (tile1 != null)
                            {
                                if (world.tiles[tile1.x, tile1.y] != null)
                                {
                                    goBack = true;
                                    break;
                                }
                                if (world.tiles[tile1.x, tile1.originalPos.y - l] == null) 
                                { 
                                    goBack = true;
                                    break;
                                }

                            }
                            else
                            {
                                goBack = true;
                                break;
                            }

                        }

                        if (goBack) continue;
                        tooClose = 4;
                        foreach (Tile tile in tileArray)
                        {
                            setTile(tile.x, tile.y, tile);
                        }

                    }
                }
            }
        }


        public void GenerateTrees()
        {
            int prevPoint = 0;

            for (int i = 0; i < forestPoints.Length; i++)
            {
                int forestPoint = forestPoints[i];
                if (heightMap[forestPoint] > 64) continue;
                if (forestPoint > 0 && forestPoint < 64)
                {
                    int height = (int)heightMap[forestPoint] + 1;
                    if (forestPoint - prevPoint < 5) continue;
                    List<Tile[,]> tileT = GenerateTree(forestPoint, height);
                    bool goBack = false;
                    if (tileT != null)
                    {
                        List<Tile> tileList = new List<Tile>();

                        foreach (Tile[,] tilet in tileT)
                        {
                            foreach (Tile tile1 in tilet)
                            {
                                if (tile1 != null)
                                {
                                    Tile tile = world.tiles[tile1.x, tile1.y];

                                    if (tile != null)
                                    {
                                        goBack = true;
                                        break;
                                    }
                                    tileList.Add(tile1);
                                }


                            }

                            if (goBack) break;
                        }

                        if (goBack) continue;
                        foreach (Tile tile in tileList) setTile(tile.x, tile.y, tile);
                        prevPoint = forestPoint;
                    }
                }
            }
        }

        public float setRotation(float x)
        {
            return MathHelper.ToRadians(x * 90);
        }




        public static Chunk CreateChunk(int X, int Y, World world, bool GenerateTiles)
        {
            Chunk newChunk = new Chunk();
            newChunk.x = X;
            newChunk.y = Y;
            newChunk.biomeToLeft = world.getBiome(X - 1, Y);
            newChunk.biomeToBelow = world.getBiome(X, Y - 1);
            newChunk.biomeToAbove = world.getBiome(X, Y + 1);
            newChunk.biomeToRight = world.getBiome(X + 1, Y);
            newChunk.world = world;
            newChunk.biome = world.getBiome(X, Y);
            newChunk.mossMap = GenerateMossMap(X, Y, world);
            newChunk.caveMap = GenerateCaveMap(X, Y, world);
            newChunk.heightMap = GenerateHeightMap(X, Y, world, newChunk.biome);
            newChunk.biome.type = Biome.GetBiomeType(newChunk.heightMap);
            newChunk.forestPoints = GenerateForestPoints(X, Y, world, newChunk.biome);
            
           
            if(GenerateTiles) newChunk.GenerateTiles();

            return newChunk;
        }


        public void setTile(int X, int Y, Tile tile)
        {
            int adjX = X - x * 64;
            int adjY = Y - y * 64;
            if(adjX > 63 || adjX < 0 || adjY > 63 || adjY < 0) {
                
                (int x, int y) pos = World.findChunk(X, Y);
                Chunk c = world.loadedChunks[pos];
                if (c == null) c = World.retrieveChunk(world.GenerateChunkFilepath(pos));
                if (c != null)
                {
                    c.tiles[X - pos.x * 64, Y - pos.y * 64] = tile;
                    tile.chunkPos = pos;
                }
                else
                {
                    PartialChunk partialChunk = world.GetPartialChunk(pos.x, pos.y);
                    if (partialChunk == null)
                    {
                        partialChunk = new PartialChunk();
                        partialChunk.x = pos.x;
                        partialChunk.y = pos.y;
                    }
                    tile.chunkPos = pos;
                    partialChunk.tiles[X - pos.x * 64, Y - pos.y * 64] = tile;
                    world.partialChunks.Add(pos, partialChunk);
                }
            } else
            {
                tiles[adjX, adjY] = tile;
            }
        }


    }
}
