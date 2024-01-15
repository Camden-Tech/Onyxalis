using Lucene.Net.Support;
using MiNET.Utils;
using Newtonsoft.Json.Linq;
using Onyxalis.Objects.Entities;
using Onyxalis.Objects.Systems;
using Onyxalis.Objects.Tiles;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static Onyxalis.Objects.Worlds.World;

namespace Onyxalis.Objects.Worlds
{
    public class World
    {
        public string name;
        public ChunkClusters clusters = new ChunkClusters();
        public LoadedTiles tiles;
        Guid uuid;
        string currentDirectory;
        string onyxalisDirectory;
        string filePath;
        public World()
        {

            uuid = Guid.NewGuid();
            name = uuid.ToString();
            currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            onyxalisDirectory = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\..\"));
            filePath = Path.Combine(onyxalisDirectory, $"Worlds/" + name);
            tiles = new LoadedTiles(this);
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
        public struct LoadedTiles
        {
            private World world;
            public LoadedTiles(World w)
            {
                world = w;
            }

            public Tile this[int x, int y]
            {
                get
                {
                    (int cX, int cY) chunk = findChunk(x, y);
                    (int tX, int tY) tileInChunk = (x - chunk.cX * 64, y - chunk.cY * 64);

                    Tile tile = null;
                    Chunk cChunk = world.loadedChunks[(chunk.cX, chunk.cY)];
                    if (cChunk != null) {
                        tile = cChunk.tiles[tileInChunk.tX, tileInChunk.tY];
                    }
                    return tile;
                }
                set
                {
                    (int cX, int cY) chunk = findChunk(x, y);
                    (int tX, int tY) tileInChunk = (x - chunk.cX * 64, y - chunk.cY * 64);
                    Tile tile = null;
                    Chunk cChunk = world.loadedChunks[(chunk.cX, chunk.cY)];
                    if (cChunk != null)
                    {
                        cChunk.tiles[tileInChunk.tX, tileInChunk.tY] = value;
                    }
                    
                    
                }
            }
        }
        

        public HashMap<(int,int), Chunk> loadedChunks = new HashMap<(int, int), Chunk>();
        public int time;
        public Weather weather;
        public int seed;
        public Random worldRandom;

        public static World CreateWorld()
        {
            World world = new();
            world.weather = new Weather(); //Generate Weather Method
            return world;
        }

        public static (int x, int y) findChunkClusterPosition(int X, int Y)
        {
            
            return ((int)MathF.Round((X-7.5f)/16f), (int)(MathF.Round((Y-7.5f) / 16f)));
        }

        public static (int x, int y) findChunk(int X, int Y)
        {
            return ((int)MathF.Round((X - 31.5f) / 64f), (int)(MathF.Round((Y - 31.5f) / 64f))); 
        }


        public void UnloadChunk((int x, int y) pos)
        {
            Chunk c = loadedChunks[pos];
            writeChunkFile(c);
            c.cluster.chunks[c.whatChunkInCluster.x, c.whatChunkInCluster.y] = null;
            loadedChunks.Remove(pos);
        }

        public string GenerateChunkFilepath((int x, int y) c)
        {
            return filePath + "/Chunk_" + c.x + "_" + c.y + ".txt";
        }

        public string writeChunkFile(Chunk c)
        {


            // Navigate up one directory
            string serializedHex = ObjectSerializer.SerializeChunk(c);
            Directory.CreateDirectory(filePath);
            string path = GenerateChunkFilepath((c.x, c.y));
            File.WriteAllText(path, serializedHex);
            return path;
        }


        public static Chunk retrieveChunk(string filePath)
        {
            // Read the Base64 encoded, compressed data from the file
            if (File.Exists(filePath))
            {
                string base64SerializedData = File.ReadAllText(filePath);

                // Use the existing DeserializeChunk method to convert the data back to a Chunk object
                return ObjectSerializer.DeserializeChunk(base64SerializedData);
            }
            return null;
        }

        public Chunk LoadChunk(int x, int y)
        {
            (int X, int Y) = findChunkClusterPosition(x, y);
            ChunkCluster cluster = clusters[X, Y];
            if(cluster == null) cluster = CreateChunkCluster(X, Y);
            int whatChunkX = x - X * 16;
            int whatChunkY = y - Y * 16;
            Chunk chunk = loadedChunks[(whatChunkX, whatChunkY)];
            if (chunk == null) chunk = retrieveChunk(GenerateChunkFilepath((x,y)));
            if (chunk == null) chunk = cluster.GenerateChunk(whatChunkX, whatChunkY, true);
            
            chunk.loaded = true;
            loadedChunks.Add((whatChunkX, whatChunkY), chunk);
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
