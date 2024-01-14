using Lucene.Net.Support;
using MiNET.Utils;
using Onyxalis.Objects.Entities;
using Onyxalis.Objects.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Worlds
{
    public class Chunk
    {
        public Tile[,] tiles = new Tile[64,64];

        public ChunkCluster cluster;
        public (int x, int y) whatChunkInCluster;

        public HashMap<UUID, LivingCreature> nonPlayers;

        public bool surfaceChunk;

        public int x;
        public int y;

        public World world;

        
        public void GenerateTiles()
        {
            for (int X = 0; X < 64; X++)
            {
                float height = cluster.heightMap[X + whatChunkInCluster.x * 64] - whatChunkInCluster.y * 64 - cluster.y * 1024;
                for (int Y = 0; Y < height && Y < 64; Y++)
                {
                    Tile tile = new Tile();
                    tile.x = X + x * 64;
                    tile.Type = Tile.TileType.DIRT;
                    tile.y = Y + y * 64;
                    tiles[X, Y] = tile; 
                }
            }
        }
        
        public static Chunk CreateChunk(int X, int Y, World world, bool GenerateTiles, bool SurfaceChunk, ChunkCluster cluster)
        {
            Chunk newChunk = new Chunk();
            newChunk.cluster = cluster;
            newChunk.whatChunkInCluster.x = X;
            newChunk.whatChunkInCluster.y = Y;
            newChunk.x = X + cluster.x * 16;
            newChunk.y = Y + cluster.y * 16;
            int seed = world.seed;
            newChunk.surfaceChunk = SurfaceChunk;
            if(GenerateTiles) newChunk.GenerateTiles();
            return newChunk;
        }
        
    }
}
