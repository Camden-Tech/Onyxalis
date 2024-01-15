using Newtonsoft.Json;
using Onyxalis.Objects.Worlds;
using Onyxalis.Objects.Tiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
namespace Onyxalis.Objects.Systems
{
    public static class ObjectSerializer
    {
        public static string SerializeChunk(Chunk chunk)
        {
            StringBuilder sb = new StringBuilder();

            // Serialize primitive types
            sb.AppendLine(chunk.surfaceChunk.ToString());
            sb.AppendLine(chunk.x.ToString());
            sb.AppendLine(chunk.y.ToString());

            // Serialize complex types (like the tiles array)
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    Tile tile = chunk.tiles[i, j];
                    if (tile != null)
                    {
                        // Serialize each tile's properties
                        sb.AppendLine($"{tile.x},{tile.y},{tile.rotation},{(int)tile.Type},{tile.chunkPos.chunkX},{tile.chunkPos.chunkY}");
                    }
                    else
                    {
                        sb.AppendLine("null");
                    }
                }
            }

            return sb.ToString();
        }

        public static Chunk DeserializeChunk(string serializedData)
        {
            var lines = serializedData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            int lineIndex = 0;

            Chunk chunk = new Chunk();

            // Deserialize primitive types
            chunk.surfaceChunk = bool.Parse(lines[lineIndex++]);
            chunk.x = int.Parse(lines[lineIndex++]);
            chunk.y = int.Parse(lines[lineIndex++]);

            // Deserialize complex types (like the tiles array)
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
                            Type = (Tile.TileType)int.Parse(parts[3])
                        };
                        chunk.tiles[i, j] = tile;
                    }
                }
            }

            return chunk;
        }


    }
}
