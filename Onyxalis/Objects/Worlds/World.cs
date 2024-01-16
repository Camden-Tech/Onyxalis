﻿using Lucene.Net.Support;
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
            Chunk chunk = loadedChunks[(x, y)];
            if (chunk == null) chunk = retrieveChunk(GenerateChunkFilepath((x,y)));
            if (chunk == null) chunk = Chunk.CreateChunk(x, y, this, true);
            
            chunk.loaded = true;
            loadedChunks.Add((x, y), chunk);
            return chunk;
        }

        public void GenerateSeed()
        {
            seed = Environment.TickCount;
            worldRandom = new Random(seed);
        }


        public Biome getBiome(int x, int y)
        {
            float biomeTypeNoise = PerlinNoiseGenerator.GeneratePerlinNoise(4, 0.25f, 1f, 1, seed, x);
            float temperature = PerlinNoiseGenerator.GeneratePerlinNoise(4, 0.25f, 1f, 1, seed, x) * 140 - 40;
            float amplitude = PerlinNoiseGenerator.GeneratePerlinNoise(4, 0.25f, 1f, 1, seed, x) * 2 + 1;

            Biome biome = new Biome(Biome.GetTerrainType(biomeTypeNoise), temperature, amplitude);
            return biome;
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
            int Y = 0; //fix this anyways
            return new Vector2((chosenSpot + chosenChunk.x * 64) * Tile.tilesize, -Y * Tile.tilesize) ;
        }
    }
}
