using Microsoft.Xna.Framework;
using Onyxalis.Objects.Tiles;
using System;

namespace Onyxalis.Objects.Networks
{
    [Serializable]
    public class TileData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Tile.TileType Type { get; set; }
        public float Rotation { get; set; }
        public Tile.Covering Covering { get; set; }
        public bool MultiTile { get; set; }
        public (int x, int y) PiecePos { get; set; }
        public (int chunkX, int chunkY) ChunkPos { get; set; }

        public TileData() { }

        public TileData(Tile tile)
        {
            X = tile.x;
            Y = tile.y;
            Type = tile.Type;
            Covering = tile.covering;
            MultiTile = tile.multiTile;
            PiecePos = tile.piecePos;
            ChunkPos = tile.chunkPos;
            Rotation = tile.rotation;
        }


        public Tile ConvertToTile()
        {
            Tile tile = new Tile();

            tile.x = this.X;
            tile.y = this.Y;
            tile.Type = this.Type;
            tile.covering = this.Covering;
            tile.multiTile = this.MultiTile;
            tile.piecePos = this.PiecePos;
            tile.chunkPos = this.ChunkPos;
            tile.rotation = this.Rotation; // Set default rotation or any other default properties
            (Tile.DigType digType, int health) = Tile.TileDictionary[tile.Type];
            tile.digType = digType;
            tile.hitbox.Position = new Microsoft.Xna.Framework.Vector2(tile.x * Tile.tilesize, tile.y * Tile.tilesize);

            return tile;
        }

        // Additional methods or properties if necessary
    }
}