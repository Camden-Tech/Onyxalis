using Lucene.Net.Support;
using MiNET.Blocks;
using MiNET.Entities.Hostile;
using MiNET.Utils;
using Newtonsoft.Json.Linq;
using Onyxalis.Objects.Entities;
using Onyxalis.Objects.Math;
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
                    Chunk cChunk = world.GetChunk(chunk.cX, chunk.cY);
                    if (cChunk != null) {
                        tile = cChunk.tiles[tileInChunk.tX, tileInChunk.tY];
                    } 
                    return tile;
                }
                set
                {
                    (int cX, int cY) chunk = findChunk(x, y);
                    (int tX, int tY) tileInChunk = (x - chunk.cX * 64, y - chunk.cY * 64);
                    Chunk cChunk = world.GetChunk(chunk.cX, chunk.cY);
                    if (cChunk != null)
                    {
                        cChunk.tiles[tileInChunk.tX, tileInChunk.tY] = value;
                    }
                    
                    
                }
            }
        }
        

        public HashMap<(int,int), Chunk> loadedChunks = new HashMap<(int, int), Chunk>();
        public HashMap<(int, int), PartialChunk> partialChunks = new HashMap<(int, int), PartialChunk>();
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

        public static (int x, int y) findTilePosition(float X, float Y)
        {
            return ((int)MathF.Round((X - (Tile.tilesize / 2 - 0.5f)) / Tile.tilesize), (int)MathF.Round((Y - (Tile.tilesize / 2 - 0.5f)) / Tile.tilesize));
        }
        public static (int x, int y) findChunk(int X, int Y)
        {
            return ((int)MathF.Round((X - 31.5f) / 64f), (int)(MathF.Round((Y - 31.5f) / 64f))); 
        }


        public void UnloadChunk((int x, int y) pos)
        {
            Chunk c = loadedChunks[pos];
            writeChunkFile(c);
            loadedChunks.Remove(pos);
        }

        public string GenerateChunkFilepath((int x, int y) c)
        {
            return filePath + "/Chunk_" + c.x + "_" + c.y + ".txt";
        }
        public string GeneratePartialChunkFilepath((int x, int y) c)
        {
            return filePath + "/PartialChunk_" + c.x + "_" + c.y + ".txt";
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
        public static PartialChunk retrievePartialChunk(string filePath)
        {
            // Read the Base64 encoded, compressed data from the file
            if (File.Exists(filePath))
            {
                string base64SerializedData = File.ReadAllText(filePath);

                // Use the existing DeserializeChunk method to convert the data back to a Chunk object
                return ObjectSerializer.DeserializePartialChunk(base64SerializedData);
            }
            return null;
        }

        public string writePartialChunkFile(PartialChunk c)
        {
            string serializedHex = ObjectSerializer.SerializePartialChunk(c);
            Directory.CreateDirectory(filePath);
            string path = GeneratePartialChunkFilepath((c.x, c.y));
            File.WriteAllText(path, serializedHex);
            return path;
        }

        public PartialChunk GetPartialChunk(int x, int y)
        {
            PartialChunk chunk = partialChunks[(x, y)];
            if (chunk == null)
            {
                chunk = retrievePartialChunk(GeneratePartialChunkFilepath((x, y)));
            }
            
            return chunk;
        }

        public void deletePartialChunk(String filePath)
        {
                // Read the Base64 encoded, compressed data from the file
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
        }


        public Chunk GetChunk(int x, int y)
        {
            Chunk chunk = loadedChunks[(x, y)];
            if (chunk == null) chunk = retrieveChunk(GenerateChunkFilepath((x, y)));
            if (chunk == null) chunk = Chunk.CreateChunk(x, y, this, true);
            return chunk;
        }


        public Chunk LoadChunk(int x, int y)
        {
            Chunk chunk = GetChunk(x, y);
            foreach ((int x, int y) pos in partialChunks.Keys)
            {
                PartialChunk partialChunk = partialChunks[pos];
                
                if (partialChunk.x == x && partialChunk.y == y)
                {
                    for (int i = 0; i < 64; i++)
                    {
                        for (int j = 0; j < 64; j++)
                        {
                            Tile tile = partialChunk.tiles[i, j];
                            if (tile != null)
                            {
                                chunk.tiles[i, j] = tile;
                            }
                            
                        }
                    }
                    deletePartialChunk(GeneratePartialChunkFilepath((x, y)));
                    partialChunks.Remove(pos);
                }
            }
            chunk.loaded = true;
            loadedChunks.Add((x, y), chunk);

            for (int X = -1; X < 2; X++)
            {
                for (int Y = -1; Y < 2; Y++)
                {
                    Chunk c = loadedChunks[(X + x,Y + y)];
                    if(c == null) continue;
                    c.adjacentChunkLoaded();
                }
            }

            return chunk;
        }

        public void GenerateSeed()
        {
            seed = Environment.TickCount;
            worldRandom = new Random(seed);
        }


        public Biome getBiome(int x, int y)
        {
            float temperature = ((PerlinNoiseGenerator.GeneratePerlinNoise(1, 1f, 0.1f, 1, seed, x)) + 0.5f) * 150 - 50;
            
            float amplitude = ((PerlinNoiseGenerator.GeneratePerlinNoise(4, 0.25f, 1, 3, seed, x) / 1.33203125f + 1.5f))
                + MathF.Pow(((PerlinNoiseGenerator.GeneratePerlinNoise(4, 0.25f, 1, 2.75f, seed + 2, x) / 1.33203125f + 1.325f)), 10);
            float hBiome = ((PerlinNoiseGenerator.GeneratePerlinNoise(1, 1f, 0.2f, 1, seed + 3, x)) + 0.5f);



            Biome biome = new Biome(Biome.GetHorizontalTerrainType(hBiome), temperature, amplitude);
            return biome;
        }

        public Vector2 GenerateSpawnLocation()
        { 
            float height = 0;
            int chosenSpot = 0;
            Chunk chunk = LoadChunk(0, 0);
                    
            height = chunk.heightMap[0] - chunk.y * 64 + 8;
                      
            
            int Y = (int)height; //fix this anyways
            return new Vector2((chosenSpot + chunk.x * 64) * Tile.tilesize, -Y * Tile.tilesize) ;
        }
    }
}
