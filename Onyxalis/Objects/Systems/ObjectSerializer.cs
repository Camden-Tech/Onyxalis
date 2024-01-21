using Newtonsoft.Json;
using Onyxalis.Objects.Worlds;
using Onyxalis.Objects.Tiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Diagnostics.Metrics;
using AStarNavigator;
using Tile = Onyxalis.Objects.Tiles.Tile;

namespace Onyxalis.Objects.Systems
{
    public static class ObjectSerializer
    {
        private static string CompressToBase64(string input)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(input);

            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gzipStream.Write(byteArray, 0, byteArray.Length);
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
        public static string SerializePartialChunk(PartialChunk partialChunk)
        {
            StringBuilder sb = new StringBuilder();

            // Serialize primitive types
            sb.AppendLine(partialChunk.x.ToString());
            sb.AppendLine(partialChunk.y.ToString());

            // Serialize the tiles array
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    Tile tile = partialChunk.tiles[i, j];
                    if (tile != null)
                    {
                        // Serialize each tile's properties
                        sb.AppendLine($"{tile.x},{tile.y},{tile.rotation},{(int)tile.Type},{tile.chunkPos.chunkX},{tile.chunkPos.chunkY}, {(int)tile.covering}, {tile.health}, {tile.piecePos.x}, {tile.piecePos.y}");
                    }
                    else
                    {
                        sb.AppendLine("null");
                    }
                }
            }

            // Compress and convert to Base64
            return CompressToBase64(sb.ToString());
        }

        public static PartialChunk DeserializePartialChunk(string base64SerializedData)
        {
            byte[] compressedData = Convert.FromBase64String(base64SerializedData);

            // Decompress the data
            using (var compressedStream = new MemoryStream(compressedData))
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var decompressedStream = new MemoryStream())
            {
                gzipStream.CopyTo(decompressedStream);
                byte[] decompressedData = decompressedStream.ToArray();

                // Convert the decompressed byte array back to a string
                string serializedData = Encoding.UTF8.GetString(decompressedData);

                var lines = serializedData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                int lineIndex = 0;

                PartialChunk chunk = new PartialChunk();

                // Deserialize primitive types
                chunk.x = int.Parse(lines[lineIndex++]);
                chunk.y = int.Parse(lines[lineIndex++]);
                
                // Deserialize the tiles array
                for (int i = 0; i < 64; i++)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        string tileData = lines[lineIndex++];
                        if (tileData != "null")
                        {
                            var parts = tileData.Split(',');
                            Tile tile = new Tile
                            {
                                x = int.Parse(parts[0]),
                                y = int.Parse(parts[1]),
                                rotation = int.Parse(parts[2]),
                                Type = (Tile.TileType)int.Parse(parts[3]),
                                chunkPos = (int.Parse(parts[4]), int.Parse(parts[5])),
                                covering = (Tile.Covering)int.Parse(parts[6]),
                                health = int.Parse(parts[7]),
                                piecePos = (int.Parse(parts[8]), int.Parse(parts[9]))
                            };
                            // Assuming TileDictionary is accessible
                            if (tile.piecePos != (-1,-1))
                            {
                                tile.multiTile = true;
                                tile.originalPos = (tile.x - tile.piecePos.x, tile.y - tile.piecePos.y);
                            }
                            (Tile.DigType digType, int health) = Tile.TileDictionary[tile.Type];
                            tile.digType = digType;
                            tile.hitbox.Position = new Microsoft.Xna.Framework.Vector2(tile.x * Tile.tilesize, tile.y * Tile.tilesize);
                            chunk.tiles[i, j] = tile;
                        }
                    }
                }
                // Deserialize mossMap, heightMap, caveMap, and forestPoints as needed
                // DeserializeMossMap, DeserializeHeightMap, DeserializeCaveMap, DeserializeForestPoints can be implemented similarly

                return chunk;
            }
        }

        public static string SerializeChunk(Chunk chunk)
        {
            StringBuilder sb = new StringBuilder();

            // Serialize primitive types
            sb.AppendLine(chunk.x.ToString());
            sb.AppendLine(chunk.y.ToString());
            sb.AppendLine(chunk.generatedTrees.ToString());
            sb.AppendLine(((int)chunk.biome.type).ToString());

            // Serialize complex types (like the tiles array)
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    Tile tile = chunk.tiles[i, j];
                    if (tile != null)
                    {
                        // Serialize each tile's properties
                        sb.AppendLine($"{tile.x},{tile.y},{tile.rotation},{(int)tile.Type},{tile.chunkPos.chunkX},{tile.chunkPos.chunkY}, {(int)tile.covering}, {tile.health}, {tile.piecePos.x}, {tile.piecePos.y}");
                    }
                    else
                    {
                        sb.AppendLine("null");
                    }
                }
            }
            byte[] byteArray = Encoding.UTF8.GetBytes(sb.ToString());

            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gzipStream.Write(byteArray, 0, byteArray.Length);
                }

                // Convert the compressed data to Base64
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public static Chunk DeserializeChunk(string base64SerializedData)
        {
            byte[] compressedData = Convert.FromBase64String(base64SerializedData);

            // Decompress the data
            using (var compressedStream = new MemoryStream(compressedData))
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var decompressedStream = new MemoryStream())
            {
                gzipStream.CopyTo(decompressedStream);
                byte[] decompressedData = decompressedStream.ToArray();

                // Convert the decompressed byte array back to a string
                string serializedData = Encoding.UTF8.GetString(decompressedData);

                var lines = serializedData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                int lineIndex = 0;

                Chunk chunk = new Chunk();

                // Deserialize primitive types
                chunk.x = int.Parse(lines[lineIndex++]);
                chunk.y = int.Parse(lines[lineIndex++]);
                chunk.generatedTrees = bool.Parse(lines[lineIndex++]);
                chunk.world = Game1.world;
                chunk.biome = chunk.world.getBiome(chunk.x, chunk.y);
                chunk.biome.type = (Biome.BiomeType)int.Parse(lines[lineIndex++]);
                // Deserialize complex types (like the tiles array)
                Tile[,] newTiles = new Tile[64, 64];
                
                for (int i = 0; i < 64; i++)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        Tile newTile = newTiles[i, j];
                        if (newTile != null)
                        {
                            chunk.tiles[i, j] = newTile;
                            continue;
                        }
                        string tileData = lines[lineIndex++];
                        if (tileData != "null")
                        {
                            var parts = tileData.Split(',');
                            Tile tile = new Tile
                            {
                                x = int.Parse(parts[0]),
                                y = int.Parse(parts[1]),
                                rotation = int.Parse(parts[2]),
                                Type = (Tile.TileType)int.Parse(parts[3]),
                                chunkPos = (int.Parse(parts[4]), int.Parse(parts[5])),
                                covering = (Tile.Covering)int.Parse(parts[6]),
                                health = int.Parse(parts[7]),
                                piecePos = (int.Parse(parts[8]), int.Parse(parts[9]))
                            };
                            // Assuming TileDictionary is accessible
                            if (tile.piecePos != (-1, -1))
                            {
                                tile.multiTile = true;
                                tile.originalPos = (tile.x - tile.piecePos.x, tile.y - tile.piecePos.y);
                            }
                            (Tile.DigType digType, int health) = Tile.TileDictionary[tile.Type];
                            tile.digType = digType;
                            tile.hitbox.Position = new Microsoft.Xna.Framework.Vector2(tile.x * Tile.tilesize, tile.y * Tile.tilesize);
                            chunk.tiles[i, j] = tile;
                        }
                    }
                }
                

                return chunk;
            }
        }

    }

}
