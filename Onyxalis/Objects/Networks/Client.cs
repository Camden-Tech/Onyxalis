using Microsoft.Xna.Framework;
using MiNET.Utils;
using Newtonsoft.Json;
using Onyxalis.Objects.Tiles;
using Onyxalis.Objects.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Networks
{
    public class Client
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private string _serverIp;
        private int _serverPort;
        private World world;

        public Client(TcpClient tcpClient, string serverIp, int serverPort)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _tcpClient = tcpClient;
        }

        public async Task ConnectAsync()
        {
            try
            {
                await _tcpClient.ConnectAsync(_serverIp, _serverPort);
                _stream = _tcpClient.GetStream();
                Console.WriteLine("Connected to the server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to server: " + ex.Message);
            }
        }

        public async Task RequestChunks(Vector2 viewportCenter, int viewportWidth, int viewportHeight)
        {
            var request = new
            {
                action = "request_chunks",
                data = new
                {
                    centerX = viewportCenter.X,
                    centerY = viewportCenter.Y,
                    width = viewportWidth,
                    height = viewportHeight
                }
            };

            string jsonRequest = JsonConvert.SerializeObject(request);
            await SendAsync(jsonRequest);
        }

        private async Task SendAsync(string message)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);
        }

        public async Task ProcessServerResponse()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
            string response = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

            dynamic jsonResponse = JsonConvert.DeserializeObject(response);
            if (jsonResponse?.action == "chunk_data")
            {
                ProcessChunkData(jsonResponse.data);
            }
        }

        private void ProcessChunkData(string chunkDataJson)
        {
            // Deserialize the JSON data to a Chunk object or a suitable data structure
            var chunkData = JsonConvert.DeserializeObject<Chunk>(chunkDataJson);

            // Update the world with the received chunk data
            UpdateGameWorldWithChunk(chunkData);
        }

        private void UpdateGameWorldWithChunk(Chunk chunkData)
        {
            // Assuming chunkData contains tiles and their positions
            foreach (TileData tileData in chunkData.tiles)
            {
                // Convert tileData to Tile object and update the world
                Tile tile = ConvertToTile(tileData);
                world.tiles[tile.x, tile.y] = tile;
            }
        }

        private Tile ConvertToTile(TileData tileData)
        {
            // Logic to convert tile data received from server to Tile object
            // Example: new Tile { Position = tileData.Position, Type = tileData.Type, ... }
            // Return the constructed Tile object
        }



        private void UpdateGameWorld(List<Tile> tiles)
        {
            // Logic to update the game world with new tiles
        }

        public void Disconnect()
        {
            _tcpClient.Close();
            Console.WriteLine("Disconnected from server.");
        }
    }
}
