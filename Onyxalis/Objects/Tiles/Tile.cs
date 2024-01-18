using AStarNavigator;
using Microsoft.Xna.Framework;
using Onyxalis.Objects.Math;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Tiles
{
    public class Tile
    {
        public const int tilesize = 16;

        public static Dictionary<TileType, (DigType digType, int health)> TileDictionary = new Dictionary<TileType, (DigType digType, int health)>(){
            {TileType.GRASS, (DigType.Digging, 10)},
            {TileType.GRASS2, (DigType.Digging, 10)},
            {TileType.DIRT1, (DigType.Digging, 8)},
            {TileType.DIRT2, (DigType.Digging, 8)},
            {TileType.DIRT3, (DigType.Digging, 8)},
            {TileType.DIRT4, (DigType.Digging, 8)},
            {TileType.STONE, (DigType.Crushing, 15)},
            {TileType.DEEPROCK1, (DigType.Crushing, 20)},
            {TileType.DEEPROCK2, (DigType.Crushing, 20)},
            {TileType.DEEPROCK3, (DigType.Crushing, 20)},
            {TileType.DEEPROCK4, (DigType.Crushing, 20)},
            {TileType.PERMAFROST1, (DigType.Digging, 30)},
            {TileType.PERMAFROST2, (DigType.Digging, 30)},
            {TileType.PERMAFROST3, (DigType.Digging, 30)},
            {TileType.PERMAFROST4, (DigType.Digging, 30)},
            {TileType.SNOW1, (DigType.Digging, 5)},
            {TileType.SNOW2, (DigType.Digging, 5)},
            {TileType.SNOW3, (DigType.Digging, 5)},
            {TileType.SNOW4, (DigType.Digging, 5)},
            {TileType.SAND1, (DigType.Digging, 8)},
            {TileType.SAND2, (DigType.Digging, 8)},
            {TileType.SAND3, (DigType.Digging, 8)},
            {TileType.SAND4, (DigType.Digging, 8)},
            {TileType.SHRUB, (DigType.Cutting, 4)},
            {TileType.SHORTGRASS, (DigType.Cutting, 2)},
            {TileType.TALLGRASS, (DigType.Cutting, 3)},
            {TileType.COPPERDEEPROCK, (DigType.Crushing, 35)},
            {TileType.WOOD, (DigType.Cutting, 10)}
        };
        
        public enum DigType
        {
            Digging,
            Cutting,
            Crushing,
            None,
        }
        
        public enum TileType
        {
            GRASS = 0,
            GRASS2 = 1,

            DIRT1 = 2,
            DIRT2 = 3,
            DIRT3 = 4,
            DIRT4 = 5,

            STONE = 6,

            DEEPROCK1 = 7,
            DEEPROCK2 = 8,
            DEEPROCK3 = 9,
            DEEPROCK4 = 10,

            PERMAFROST1 = 11,
            PERMAFROST2 = 12,
            PERMAFROST3 = 13,
            PERMAFROST4 = 14,

            SAND1 = 15,
            SAND2 = 16,
            SAND3 = 17,
            SAND4 = 18,

            SNOW1 = 19,
            SNOW2 = 20,
            SNOW3 = 21,
            SNOW4 = 22,

            SHRUB = 23,

            SHORTGRASS = 24,
            TALLGRASS = 25,
            
            COPPERDEEPROCK = 26,

            WOOD = 27
        }

        public enum Covering
        {
            NONE = 0,
            MOSS = 1
        }

        public Tile()
        {
            hitbox = new Hitbox(new Vector2[] { new Vector2(0, 0), new Vector2(Tile.tilesize, 0), new Vector2(Tile.tilesize, -Tile.tilesize), new Vector2(0, -Tile.tilesize) }, Vector2.Zero, 2);
        }
        //Each tile is 8x8
        public int health;
        public Hitbox hitbox;
        public DigType digType;
        public int x;
        public Covering covering;
        public int y;
        public (int chunkX, int chunkY) chunkPos;
        public int rotation;
        public TileType Type;
        
        
        
        public bool damageTile(int amount, DigType type) {
            health -=  amount * (int)(type == digType ? 1.5f : 1);
            if(health <= 0) {
                return true;
            }
            return false;
        }

    
    }
}
