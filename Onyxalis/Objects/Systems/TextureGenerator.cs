using System;
using System.Drawing;

class static TextureGenerator
{
    public static bool AreTexturesSameSize(Texture2D texture1, Texture2D texture2)
    {
        return texture1.Width == texture2.Width && texture1.Height == texture2.Height;
    }

    public static Texture2D GenerateTexture(Texture2D baseTexture, Texture2D overlayTexture)
    {
        Color[] baseColors = new Color[baseTexture.Width * baseTexture.Height];
        Color[] overlayColors = new Color[overlayTexture.Width * overlayTexture.Height];

        baseTexture.GetData(baseColors);
        overlayTexture.GetData(overlayColors);

        // Create a new array to store the generated result
        Color[] resultColors = new Color[baseColors.Length];

        // Loop through each pixel and apply overlay
        for (int i = 0; i < baseColors.Length; i++)
        {
            Color baseColor = baseColors[i];
            Color overlayColor = overlayColors[i];

            // Modify base color using overlay color (you can customize this blending method)
            Color blendedColor = BlendColors(baseColor, overlayColor);

            // Set the result color to the new blended color
            resultColors[i] = blendedColor;
        }

        // Create a new Texture2D and set the data
        Texture2D resultTexture = new Texture2D(GraphicsDevice, baseTexture.Width, baseTexture.Height);
        resultTexture.SetData(resultColors);

        return resultTexture;
    }

    public static Color BlendColors(Color baseColor, Color overlayColor)
    {
        // You can implement your own blending logic here
        // For simplicity, let's just multiply the base color by the overlay color
        int newRed = (int)(baseColor.R * overlayColor.R);
        int newGreen = (int)(baseColor.G * overlayColor.G);
        int newBlue = (int)(baseColor.B * overlayColor.B);

        return new Color(newRed, newGreen, newBlue);
    }

    void static SaveTexture(Texture2D texture, string fileName)
    {
        using (FileStream stream = new FileStream(fileName + ".png", FileMode.Create))
        {
            texture.SaveAsPng(stream, texture.Width, texture.Height);
        }
    }
}
