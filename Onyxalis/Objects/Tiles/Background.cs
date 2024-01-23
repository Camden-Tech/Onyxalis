using Microsoft.Xna.Framework;
using Onyxalis.Objects.Math;
using Onyxalis.Objects.Systems;
using Onyxalis.Objects.Worlds;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Tiles
{
    public class Background
    {
        public Tile.TileType type;
        public int x;
        public int y;
        public bool hasColor;
        public Color lightColor;
        public Color sunLight;

        public void UpdateBackgroundLighting(List<Light> lightTiles, World world)
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
            lightColor = TextureGenerator.addColors(finalColor, new Color(0,0,50));

        }
        public void UpdateBackgroundSunLighting(World world, Light sun)
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

    }
}
