using AStarNavigator;
using Microsoft.Xna.Framework;
using Onyxalis.Objects.Math;
using Onyxalis.Objects.Systems;
using Onyxalis.Objects.Worlds;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Mozilla;
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

        public static Dictionary<TileType, (DigType digType, int health)> TileDictionary = new Dictionary<TileType, (DigType digType, int health)>(){  //Create json file reader instead
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
            {TileType.LONGGRASS, (DigType.Cutting, 3)},
            {TileType.COPPERDEEPROCK, (DigType.Crushing, 35)},
            {TileType.TREESTUMP, (DigType.Cutting, 30)},
            {TileType.TREESTALK, (DigType.Cutting, 30)},
            {TileType.TREETOP, (DigType.Cutting, 30)},
            {TileType.TREETOP2, (DigType.Cutting, 30)},
        };

        public static List<TileType> transparentTiles = new List<TileType>()
        {
            TileType.LONGGRASS,
            TileType.SHRUB,
            TileType.SHORTGRASS,
            TileType.TREETOP,
            TileType.TREESTALK,
            TileType.TREESTUMP,
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

            
            COPPERDEEPROCK = 23,

            WOOD = 24,

            TREESTUMP = 25,
            TREESTALK = 26,
            TREETOP = 27,
            TREETOP2 = 28,
            SHRUB = 29,
            SHORTGRASS = 30,
            LONGGRASS = 31
        }

        public enum Covering
        {
            NONE = 0,
            MOSS = 1
        }
        
        //Each tile is 8x8
        public int health;
        public Hitbox hitbox = new Hitbox(new Vector2[] { new Vector2(0, 0), new Vector2(Tile.tilesize, 0), new Vector2(Tile.tilesize, -Tile.tilesize), new Vector2(0, -Tile.tilesize) }, Vector2.Zero, 2);
        public DigType digType;
        public int x;
        public Covering covering;
        public int y;
        public bool multiTile = false;
        public (int x, int y) piecePos = (-1, -1);
        public (int x, int y) originalPos;
        public (int chunkX, int chunkY) chunkPos;
        public float rotation;
        public TileType Type;
        public Color lightColor = Color.White;
        public Light light;
        public Color sunLight;
        
        public bool hasColor = false;

        public void UpdateTileLighting(List<Light> lightTiles, World world)
        {
            Color finalColor = sunLight; // Base ambient light
            foreach (Light light in lightTiles)
            {
                float dx = x - light.x;
                float dy = y - light.y;
                float distanceSquared = dx * dx + dy * dy;
                float lightRangeSquared = light.range * light.range;
                if (distanceSquared < lightRangeSquared)
                {
                    int level = Shadows.IsLineOfSightClear((x, y), (light.x, light.y), world, light.intensity);
                    float maxLevel = light.intensity * 4;
                    if (level <= maxLevel)
                    {
                        float attenuation = 1 - (MathF.Sqrt(distanceSquared) / light.range);
                        Color lightColor = Color.Multiply(light.color, attenuation * (1 - (level / maxLevel)) * light.intensity / 2);
                        lightColor.A = 255;
                        finalColor = TextureGenerator.addColors(finalColor, lightColor);
                    }
                }
            }
            lightColor = finalColor;

        }
        public void UpdateTileSunLighting(World world, Light sun)
        {
            Color finalColor = new Color(0, 0, 0); // Base ambient light
            if (world.day && y > -120)
            {
                float dx = x - sun.x;
                float dy = y - sun.y;
                float distanceSquared = dx * dx + dy * dy;
                float lightRangeSquared = sun.range * sun.range;

                if (distanceSquared < lightRangeSquared)
                {
                    int level = Shadows.IsLineOfSightClear((x, y), (sun.x, sun.y), world, sun.intensity);
                    float maxLevel = sun.intensity * 4;
                    if (level + 1 <= maxLevel)
                    {
                        float attenuation = 1 - ((dx + dy) / sun.range);
                        Color lightColor = Color.Multiply(sun.color, attenuation * (1 - (level / maxLevel)) * sun.intensity * 4 / maxLevel);
                        lightColor.A = 255;
                        finalColor = TextureGenerator.addColors(finalColor, lightColor);
                    }
                }
            }

            sunLight = finalColor;
            hasColor = true;
        }
        public bool damageTile(int amount, DigType type) {
            health -= amount * (int)(type == digType ? 1.5f : 1);
            if(health <= 0) {
                return true;
            }
            return false;
        }

    
    }
}
