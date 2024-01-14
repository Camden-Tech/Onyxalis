using Newtonsoft.Json.Linq;
using Onyxalis.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Onyxalis.Objects.Worlds.World;

namespace Onyxalis.Objects.Worlds
{
    public class World
    {
        public ChunkClusters clusters = new ChunkClusters();
        Tiles tiles;
        public World()
        {
            tiles = new Tiles(this);
        }

        
        public struct ChunkClusters
        {
            private ChunkCluster[,] chunkArray;
            private const int Offset = 50;
            public ChunkClusters()
            {
                chunkArray = new ChunkCluster [100, 100];
            }

            private (int, int) MapToWorldIndices(int x, int y)
            {
                return (x + Offset, y + Offset);
            }
            public ChunkCluster this[int x, int y]
            {
                get
                {
                    (int arrayX, int arrayY) = MapToWorldIndices(x, y);
                    if (arrayX < 0 || arrayX >= chunkArray.GetLength(0) || arrayY < 0 || arrayY >= chunkArray.GetLength(1))
                    {
                        throw new IndexOutOfRangeException("Coordinates are out of bounds.");
                    }
                    return chunkArray[arrayX, arrayY];
                }
                set
                {
                    (int arrayX, int arrayY) = MapToWorldIndices(x, y);
                    if (arrayX< 0 || arrayX >= chunkArray.GetLength(0) || arrayY< 0 || arrayY >= chunkArray.GetLength(1))
                    {
                        throw new IndexOutOfRangeException("Coordinates are out of bounds.");
                    }
                    chunkArray[arrayX, arrayY] = value;
                }
            }
        }
        public struct Tiles
        {
            private World world;
            public Tiles(World w)
            {
                world = w;
            }

            public Tile this[int x, int y]
            {
                get
                {
                    (int cX, int cY) chunk = findChunk(x, y);
                    (int clX, int clY) cluster = findChunkClusterPosition(chunk.cX, chunk.cY);
                    (int whatCX, int whatCY) = (chunk.cX - cluster.clX * 16, chunk.cY - cluster.clY * 16);
                    (int tX, int tY) tileInChunk = (x - chunk.cX * 64, y - chunk.cY * 64);
                    ChunkCluster cCluster = world.clusters[cluster.clX, cluster.clY];
                    Chunk cChunk = cCluster.chunks[whatCX, whatCY];
                    Tile tile = cChunk.tiles[tileInChunk.tX, tileInChunk.tY];
                    return tile;
                }
                set
                {
                    (int cX, int cY) chunk = findChunk(x, y);
                    (int clX, int clY) cluster = findChunkClusterPosition(chunk.cX, chunk.cY);
                    (int whatCX, int whatCY) = (chunk.cX - cluster.clX * 16, chunk.cY - cluster.clY * 16);
                    (int tX, int tY) tileInChunk = (x - chunk.cX * 64, y - chunk.cY * 64);
                    ChunkCluster cCluster = world.clusters[cluster.clX, cluster.clY];
                    Chunk cChunk = cCluster.chunks[whatCX, whatCY];
                    cChunk.tiles[tileInChunk.tX, tileInChunk.tY] = value;
                }
            }
        }
        

        public List<(int,int, ChunkCluster)> loadedChunks = new List<(int, int, ChunkCluster)>();
        public int time;
        public Weather weather;
        public int seed;
        public Random worldRandom;

        public static World CreateWorld()
        {
            World world = new();
            world.GenerateSeed();
            world.weather = new Weather(); //Generate Weather Method
            return world;
        }

        public static (int x, int y) findChunkClusterPosition(int X, int Y)
        {
            
            return ((int)MathF.Round((X-7.5f)/16f), (int)(MathF.Round((Y-7.5f) / 16f)));
        }

        public static (int x, int y) findChunk(int X, int Y)
        {
            return (X / 64, Y / 64); 
        }

        public void GenerateSeed()
        {
            seed = Environment.TickCount;
            worldRandom = new Random(seed);
        }


        public Chunk LoadChunk(int x, int y)
        {
            (int X, int Y) = findChunkClusterPosition(x, y);
            ChunkCluster cluster = clusters[X, Y];
            if (cluster == null)
            {
                cluster = CreateChunkCluster(X,Y);
            }
            int whatChunkX = x - X * 16;
            int whatChunkY = y - Y * 16;
            Chunk chunk = cluster.chunks[whatChunkX, whatChunkY];
            if (chunk == null)
            {
                chunk = cluster.GenerateChunk(whatChunkX, whatChunkY, true);
            }
            loadedChunks.Remove((whatChunkX,whatChunkY,cluster));
            loadedChunks.Add((whatChunkX, whatChunkY, cluster));
            return chunk;
        }
        public ChunkCluster CreateChunkCluster(int x, int y)
        {
            ChunkCluster cluster = new ChunkCluster();
            cluster.world = this;
            cluster.GenerateHeightMap();
            cluster.x = x; 
            cluster.y = y;
            clusters[x,y] = cluster;
            return cluster;
        }


        public Vector2 GenerateSpawnLocation()
        { 
            Chunk chosenChunk = null;
            for (int x = -5; x < 5; x++)
            {
                for (int y = -5; y < 5; y++)
                {
                    chosenChunk = LoadChunk(x, y);
                }
            }
            int chosenSpot = Game1.GameRandom.Next(64);
            int Y = ((int)chosenChunk.cluster.heightMap[chosenChunk.whatChunkInCluster.x * 16 + chosenSpot]) - chosenChunk.whatChunkInCluster.y * 64 - 4;
            return new Vector2(chosenSpot + chosenChunk.x * 64, Y) * Tile.tilesize;
        }
    }
}
