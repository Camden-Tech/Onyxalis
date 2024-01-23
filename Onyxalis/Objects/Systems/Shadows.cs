using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Onyxalis.Objects.Math;
using Onyxalis.Objects.Tiles;
using Onyxalis.Objects.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Systems
{
    public class Shadows
    {
        public static Texture2D ApplyColorToTexture(GraphicsDevice graphicsDevice, Texture2D originalTexture, Color colorToApply)
        {
            // Get the color data of the texture
            Color[] originalColorData = new Color[originalTexture.Width * originalTexture.Height];
            originalTexture.GetData(originalColorData);

            // Create an array to hold the modified color data
            Color[] modifiedColorData = new Color[originalColorData.Length];

            // Modify each color in the original texture
            for (int i = 0; i < originalColorData.Length; i++)
            {
                // Blend the original color with the new color
                modifiedColorData[i] = BlendColors(originalColorData[i], colorToApply);
            }

            // Create a new texture and set the modified color data
            Texture2D modifiedTexture = new Texture2D(graphicsDevice, originalTexture.Width, originalTexture.Height);
            modifiedTexture.SetData(modifiedColorData);

            return modifiedTexture;
        }

        public static Color BlendColors(Color originalColor, Color colorToApply)
        {
            // Simple blend: average of original and new color. Modify blending logic as needed
            return new Color(
                (originalColor.R + colorToApply.R) / 2,
                (originalColor.G + colorToApply.G) / 2,
                (originalColor.B + colorToApply.B) / 2,
                originalColor.A // Preserving the original alpha value
            );
        }
        public static Color BlendColors(Color originalColor, Color colorToApply, float bias)
        {
            // Simple blend: average of original and new color. Modify blending logic as needed
            float biasAdj = 1 - bias;
            
            return new Color(
                (originalColor.R * biasAdj + colorToApply.R * bias) / 2,
                (originalColor.G * biasAdj + colorToApply.G * bias) / 2,
                (originalColor.B * biasAdj + colorToApply.B * bias) / 2,
                originalColor.A // Preserving the original alpha value
            );
        }
        public static int IsLineOfSightClear((int x, int y) start, (int x, int y) end, World world, float intensity)
        {
            int x = start.x;
            int y = start.y;
            int dx = (int)MathF.Abs(end.x - start.x);
            int dy = (int)-MathF.Abs(end.y - start.y);
            int sx = start.x < end.x ? 1 : -1;
            int sy = start.y < end.y ? 1 : -1;
            int err = dx + dy; // Note the change here
            int level = 0;

            while (true)
            {
                if (TileBlocksLight(x, y, world))
                {
                    level++;
                    if (level > intensity * 4 - 1)
                    {
                        return level; // If the light level exceeds the threshold, return the current level
                    }
                }

                if (x == end.x && y == end.y)
                {
                    break;
                }

                int e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x += sx;
                }
                if (e2 <= dx)
                {
                    err += dx;
                    y += sy;
                }
            }

            return level;
        }

        private static bool TileBlocksLight(int x, int y, World world)
        {
            // Implement your logic to determine if the tile at (x, y) blocks light
            // For example, check if the tile is opaque or not
            Tile tile = world.tiles[x, y]; // Implement GetTileAt to retrieve the tile at given coordinates
            if (tile != null) {
                if (tile.multiTile == true) return false;
                return true;
            } else
            {
                return false;
            }
        }
    }
}
