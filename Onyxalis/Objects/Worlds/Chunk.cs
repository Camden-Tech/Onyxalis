using Icaria.Engine.Procedural;
using Lucene.Net.Search;
using Lucene.Net.Support;
using Maybe.SkipList;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Effects;
using MiNET.Items;
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
        public int[] forestPoints = new int[64];
        public int x;
        public int y;

        public Biome biome;
        public bool loaded;
        public World world; //Do not serialize

        public static int[] GenerateForestPoints(int x, int y, World world, Biome biome)
        {
            int[] forestPoints = new int[64];
            int i = 0;
            for (int X = 0; X < 64; X++)
            {
                if (PerlinNoiseGenerator.GeneratePerlinNoise(4, 0.25f, 0.04f, 1f, world.seed, x * 64 + X) / 1.33203125f + 0.5f > (0.8f - (biome.horizontalType == Biome.HorizontalBiomeType.Forest ? 0.4f : 0)))
                {
                    forestPoints[i] = X;
                }
            }
            return forestPoints;
        }


        public static float[] GenerateHeightMap(int x, int y, World world)
        {
            float[] perlinNoise = PerlinNoiseGenerator.GeneratePerlinNoise(64, 4, 0.25f, 1f, 1, world.seed, x * 64 - 1);
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
                if (height + 1 - Y < 1) adjustedNoiseGap = 0.075f;
                else adjustedNoiseGap = 0.075f / (height + 1 - Y);
                if (!(caveNoise > 0.2 + adjustedNoiseGap) || !(caveNoise < 0.4 - adjustedNoiseGap))
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

        public Tile[] createMultiTile(int x, int y, (int x, int y) chunkPos, int rotation, Tile.TileType type)
        {
            List<Tile> tiles = new List<Tile>();
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
            for (int X = 0; X < parts.GetLength(0); X++)
            {
                for (int Y = 0; Y < parts.GetLength(1); Y++)
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
                        multiTile = true,
                        originalPos = (adjX, adjY),
                        piecePos = (X, Y),
                        covering = Tile.Covering.NONE
                    };
                    (Tile.DigType digType, int health) = Tile.TileDictionary[tile.Type];
                    tile.health = health;
                    tile.digType = digType;
                    tile.hitbox.Position = new Microsoft.Xna.Framework.Vector2(tile.x * Tile.tilesize, tile.y * Tile.tilesize);
                    tiles.add(tile);
                }
            }
            return tiles.ToArray();
        }




        private Tile GenerateTile(int X, int Y)
        {
            float height = heightMap[X];
            float noise = caveMap[X, Y]; 
            int adjustedY = y * 64 + Y;
            float adjustedNoiseGap = 0;
            float diff = height + y * 64 - adjustedY;
            if(diff > 120) adjustedNoiseGap = 0;
            else adjustedNoiseGap = 0.08f * (1 - diff / 120);
            Tile tile;
            if (!(noise > 0.4 + adjustedNoiseGap) || !(noise < 0.6 - adjustedNoiseGap))
            { //Is not cave empty space?
                
                bool freezing = biome.temperature < 0;
                bool hot = biome.temperature > 90;
                
                int intHeight = (int)height
                
                bool belowHeightMap = height - Y <= 0;
                if (!belowHeightMap)
                { // If it is below the height
                    bool matchesHeight = Y == intHeight;
                    if (matchesHeight)
                    {

                        if (height <= 75f)
                        {
                            if (!freezing && !hot)
                            {
                                return createTile(x, y, (X, Y), 0, Tile.TileType.GRASS);
                            }

                            if (freezing)
                            {
                                tile = GenerateSnow(x,y);
                                return tile;
                            }
                            tile = GenerateSand(x, y);
                            return tile;

                        }
                    }
                    if (Y < intHeight - 40)
                    {
                        if (Y < intHeight - 160)
                        {
                            tile = GenerateDeepRock(x, y);
                            bool mossHospitable = (biome.temperature > 20 && biome.temperature < 80);
                            if (mossHospitable)
                            {
                                tile.covering = GenerateMoss(X, Y);
                            }
                            return tile;
                        }
                        tile = freezing ? GeneratePermafrost(x, y) : GenerateStone(x, y);
                        return tile;
                    }
                    int dif = intHeight + y * 64;
                    if (intHeight + y * 64 > 75)
                    {
                        if (matchesHeight)
                        {
                            tile = GenerateSnow(x, y);
                            return tile;
                        }

                        if (world.worldRandom.NextDouble() / (dif - 75) < 0.05)
                        {
                            tile = GenerateStone(x, y);
                            return tile;
                        }

                        tile = GenerateDirt(x, y);
                        return tile;

                    }


                    tile = hot ? GenerateSand(x, y) : GenerateDirt(x, y);
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

        public Tile[] GenerateGrass(int X, int Y)
        {
            if (world.worldRandom.Next(5) > 3)
            {
                Tile.TileType type = getTileTypeBetweenTileTypes(Tile.TileType.SHORTGRASS, Tile.TileType.LONGGRASS);
                int rotation = 0;
                Tile[] tiles = createMultiTile(X, Y, (x, y), rotation, type);
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

        public Tile[] GenerateFoliage(int X,int Y, float height){
            bool freezing = biome.temperature < 0;
            bool hot = biome.temperature > 90;
            Tile[] tiles;
            if (height < 75f)
            {
                if (!freezing && !hot)
                {
                    tiles = GenerateGrass(X,Y);
                    return tile;
                }
                if (freezing && world.worldRandom.Next(11) >= 9)
                {
                    tiles = createMultiTile(X, Y, (x, y), 0, Tile.TileType.SHRUB);
                    return tiles;
                }
            }
            return null;
        }


        public void GenerateTree()
        {

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

                        setTile(tile.x, tile.y, tile);
                    }
                }
            }
            int tooClose = 0;

            float[,] caveMap = GenerateCaveMap(x, y - 1, world);

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
                    if (Y == 0)
                    {
                        if (!canGenerateTile(Y - 1, height - 1, caveMap[X, 64])) continue;
                    }
                    else
                    {
                        if (tiles[X, Y - 1] == null) continue;
                    }
                    Tile[] tileArray = GenerateFoliage(X, Y, height);
                    if (tileArray != null)
                    {
                        tooClose = 5;
                        foreach (Tile tile in tileArray){
                            setTile(tile.x,tile.y, tile);
                        }
                        
                    }
                }
            }
            int prevPoint = 0;
            for (int i = 0; i < forestPoints.Length; i++)
            {
                int forestPoint = forestPoints[i];
                if (forestPoint > 0 && forestPoint < 64)
                {
                    if (forestPoint - prevPoint < 2) continue;

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
            newChunk.forestPoints = GenerateForestPoints(X, Y, world, newChunk.biome);
            
            int seed = world.seed;
           
            if(GenerateTiles) newChunk.GenerateTiles();
            PartialChunk partialChunk = World.retrievePartialChunk(world.GeneratePartialChunkFilepath((X,Y)));
            if(partialChunk != null)
            {
                newChunk.tiles = partialChunk.tiles;
            }
            return newChunk;
        }


        public void setTile(int X, int Y, Tile tile)
        {
            (int x, int y) pos = (0,0);
            int adjX = X + x * 64;
            int adjY = Y + y * 64;
            if(X > adjX + 63 || X < adjX || Y > 63 + adjY || Y < adjY){
                pos = World.findChunk(adjX, adjy);
            }
            if (pos != (0,0))
            {
                PartialChunk partialChunk = world.GetPartialChunk(pos.x, pos.y);
                if (partialChunk == null)
                {
                    partialChunk = new PartialChunk();
                    partialChunk.x = pos.x;
                    partialChunk.y = pos.y;
                }
                partialChunk.tiles[x - pos.x * 64, y - pos.y * 64] = tile;
            } else
            {
                tiles[x, y] = tile;
            }
        }


    }
}
