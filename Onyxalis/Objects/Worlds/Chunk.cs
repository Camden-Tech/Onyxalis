using Icaria.Engine.Procedural;
using Lucene.Net.Search;
using Lucene.Net.Support;
using Maybe.SkipList;
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
        public Tile[,] tiles = new Tile[64,64];

        public HashMap<UUID, LivingCreature> nonPlayers;
        
        public float[,] mossMap = new float[64,64];
        public float[] heightMap = new float[64];
        public float[,] caveMap = new float[64, 64];
        public int[] forestPoints = new int[64];
        public int x;
        public int y;

        public bool generatedTrees;
        public Biome biome;
        public bool loaded;
        public World world; //Do not serialize

        public static int[] GenerateForestPoints(int x, int y, World world, Biome biome)
        {
            int[] forestPoints = new int[64];
            int i = 0;
            if (biome.temperature > 70) return forestPoints;
            for (int X = 0; X < 64; X++)
            {
                if (PerlinNoiseGenerator.GeneratePerlinNoise(4, 0.25f, 0.04f, 1f, world.seed, x * 64 + X) / 1.33203125f + 0.5f > (0.7f - (biome.horizontalType == Biome.HorizontalBiomeType.Forest ? 0.35f : 0)))
                {
                    forestPoints[i] = X;
                    i++;
                }
            }
            return forestPoints;
        }


        public static float[] GenerateHeightMap(int x, int y, World world, Biome biome)
        {;
            float[] perlinNoise = PerlinNoiseGenerator.GeneratePerlinNoise(64, 4, 0.25f, 0.125f, biome.amplitude * 4, world.seed, x * 64);
            float[] heightMap = new float[64];

            for (int i = 0; i < 64; i++)
            {
                heightMap[i] = (perlinNoise[i] / 1.33203125f) * 5 + 32 - y * 64;
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

                float diff = height + 1 - Y;
                if (diff > 120) adjustedNoiseGap = 0;

                else adjustedNoiseGap = 0.09f * (1 - diff / 120);
                if (!(caveNoise > 0.4 + adjustedNoiseGap) || !(caveNoise < 0.6 - adjustedNoiseGap))
                {
                    return true;
                }
            }
            return false;
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
                rotation = rotation,
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
                        rotation = rotation,
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




        private Tile GenerateTile(int X, int Y)
        {
            float height = heightMap[X];
            float noise = caveMap[X, Y];
            Tile tile;
            if (canGenerateTile(Y, height, caveMap[X,Y]))
            { //Is not cave empty space?
                
                bool freezing = biome.temperature < 0;
                bool hot = biome.temperature > 80;

                int intHeight = (int)height;
                
                bool belowHeightMap = height - Y <= 0;
                if (!belowHeightMap)
                { // If it is below the height
                    bool matchesHeight = Y == intHeight;
                    if (height >= -75f)
                    {

                        if (matchesHeight)
                        {
                            if (!freezing && !hot)
                            {
                                return createTile(X, Y, (X, Y), 0, Tile.TileType.GRASS);
                            }

                            if (freezing)
                            {
                                tile = GenerateSnow(X, Y);
                                return tile;
                            }
                            tile = GenerateSand(X, Y);
                            return tile;

                        }
                    }
                    if (Y < intHeight - 40)
                    {
                        if (Y < intHeight - 160)
                        {
                            tile = GenerateDeepRock(X, Y);
                            bool mossHospitable = (biome.temperature > 20 && biome.temperature < 80);
                            if (mossHospitable)
                            {
                                tile.covering = GenerateMoss(X, Y);
                            }
                            return tile;
                        }
                        tile = freezing ? GeneratePermafrost(X, Y) : GenerateStone(X, Y);
                        return tile;
                    }
                    int add = intHeight + y * 64;
                    if (add > 75)
                    {
                        if (matchesHeight)
                        {
                            tile = GenerateSnow(X, Y);
                            return tile;
                        }

                        if (Y < intHeight - 40 + (add - 75) / 2)
                        {
                            tile = freezing ? GeneratePermafrost(X, Y) : GenerateStone(X, Y);
                            return tile;
                        }

                        tile = GenerateDirt(X, Y);
                        return tile;

                    }


                    tile = hot ? GenerateSand(X, Y) : GenerateDirt(X, Y);
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

        public Tile[,] GenerateGrass(int X, int Y)
        {
            if (world.worldRandom.Next(5) > 3)
            {
                Tile.TileType type = getTileTypeBetweenTileTypes(Tile.TileType.SHORTGRASS, Tile.TileType.LONGGRASS);
                int rotation = 0;
                Tile[,] tiles = createMultiTile(X, Y, (x, y), rotation, type);
                return tiles;
            }
            return null;
            
            
        }


        public Tile GenerateDirt(int X, int Y)
        {
            Tile.TileType type = getTileTypeBetweenTileTypes(Tile.TileType.DIRT1, Tile.TileType.DIRT4);
            int rotation = world.worldRandom.Next(4);
            Tile tile = createTile(X, Y, (x, y), rotation, type);
            return tile;
        }
        public Tile GenerateSnow(int X, int Y)
        {
            Tile.TileType type = getTileTypeBetweenTileTypes(Tile.TileType.SNOW1, Tile.TileType.SNOW4);
            int rotation = world.worldRandom.Next(4);
            Tile tile = createTile(X, Y, (x, y), rotation, type);
            return tile;
        }
        public Tile GeneratePermafrost(int X, int Y)
        {
            Tile.TileType type = getTileTypeBetweenTileTypes(Tile.TileType.PERMAFROST1, Tile.TileType.PERMAFROST4);
            int rotation = world.worldRandom.Next(4);
            Tile tile = createTile(X, Y, (x, y), rotation, type);
            return tile;
        }
        public Tile GenerateSand(int X, int Y)
        {
            Tile.TileType type = getTileTypeBetweenTileTypes(Tile.TileType.SAND1, Tile.TileType.SAND4);
            int rotation = world.worldRandom.Next(4);
            Tile tile = createTile(X, Y, (x, y), rotation, type);

            return tile;
        }
        public Tile GenerateStone(int X, int Y)
        {
            Tile.TileType type = Tile.TileType.STONE;
            
            int rotation = world.worldRandom.Next(4);
            Tile tile = createTile(X, Y, (x, y), rotation, type);
            return tile;
        }
        public Tile GenerateDeepRock(int X, int Y)
        {
            Tile.TileType type = getTileTypeBetweenTileTypes(Tile.TileType.DEEPROCK1, Tile.TileType.DEEPROCK4);
            int rotation = world.worldRandom.Next(4);
            Tile tile = createTile(X, Y, (x, y), rotation, type);
            return tile;
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
                if (freezing && world.worldRandom.Next(11) >= 9)
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
                else if (i == height) tiles = createMultiTile(X, Y + i * 4, (this.x, this.y), 0, Tile.TileType.TREETOP);
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
            for (int X = 0; X < 64; X++)
            {
                for (int Y = 0; Y < 64; Y++)
                {
                    Tile tile = GenerateTile(X, Y);
                    if (tile != null)
                    {

                        tiles[X, Y] = tile;
                    }
                }
            }



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
                        foreach (Tile tile1 in tileArray) {
                            if (tile1 != null)
                            {
                                if (world.tiles[tile1.x, tile1.y] != null)
                                {
                                    goBack = true;
                                } 
                                if (world.tiles[tile1.x, tile1.originalPos.y - tileArray.GetLength(1)] == null) goBack = true;
                                
                            } else
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

        

        public static Chunk CreateChunk(int X, int Y, World world, bool GenerateTiles)
        {
            Chunk newChunk = new Chunk();
            newChunk.x = X;
            newChunk.y = Y;
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
            if(adjX > 63 || adjX < 0 || adjY > 63 || adjY < 0){
                
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
