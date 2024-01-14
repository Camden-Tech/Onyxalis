using Newtonsoft.Json.Linq;
using Onyxalis.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class World
    {
        public Chunks chunks;
        public struct Chunks
        {
            private Chunk[,] chunkArray;
            private const int Offset = 50;
            public Chunks()
            {
                chunkArray = new Chunk[100, 100];
            }

            private (int, int) MapToWorldIndices(int x, int y)
            {
                return (x + Offset, y + Offset);
            }
            public Chunk this[int x, int y]
            {
                get
                {
                    (int arrayX, int arrayY) = MapToWorldIndices(x, y);
                    if (arrayX< 0 || arrayX >= chunkArray.GetLength(0) || arrayY< 0 || arrayY >= chunkArray.GetLength(1))
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
        



        public List<(int,int)> loadedChunks = new List<(int, int)>();
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

        


        public void GenerateSeed()
        {
            seed = Environment.TickCount;
            worldRandom = new Random(seed);
        }


        public Chunk LoadChunk(int x, int y)
        {
            Chunk chunk = chunks[x, y];
            if (chunk == null)
            {
                if (y == 0)
                { 
                    chunk = GenerateChunk(x, y, true);
                } else
                {
                    chunk = GenerateChunk(x, y, false);
                }
                
            }
            loadedChunks.Add((x, y));
            return chunk;
        }

        public Chunk GenerateChunk(int x, int y, bool surfaceChunk)
        {
            Chunk newChunk = Chunk.CreateChunk(this, true, surfaceChunk); //Create CreateChunk() method, the boolean decides whether the chunk generates terrain or not.
            chunks[x, y] = newChunk;
            return newChunk;
        }

        public Vector2 GenerateSpawnLocation()
        {
            Chunk[] viableChunks = new Chunk[10];
            for (int x = -5; x < 5; x++)
            {
                for (int y = -5; y < 5; y++)
                {
                    Chunk loadedChunk = LoadChunk(x, y);
                    if (y == 0) viableChunks[x] = loadedChunk;
                }
            }
            Chunk chosenChunk = viableChunks[Game1.GameRandom.Next(10)];
            int chosenSpot = Game1.GameRandom.Next(64);
            int Y = ((int)chosenChunk.heightMap[chosenSpot]) + 8;
            return new Vector2(chosenSpot, Y); ;
        }
    }
}
