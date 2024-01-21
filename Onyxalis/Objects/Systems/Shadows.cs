using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    }
}
