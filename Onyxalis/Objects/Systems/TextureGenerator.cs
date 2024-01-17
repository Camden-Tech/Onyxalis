using System;
using System.Drawing;

class TextureGenerator
{
    /*static void Main()
    {
        // Load base and overlay textures
        Bitmap baseTexture = new Bitmap("base_texture.png");
        Bitmap overlayTexture = new Bitmap("overlay_texture.png");
        if (!AreTexturesSameSize(baseTexture, overlayTexture))
        {
            Console.WriteLine("Base and overlay textures must have the same dimensions.");
            return;
        }

        // Generate the result texture
        Bitmap resultTexture = GenerateTexture(baseTexture, overlayTexture);

        // Save the result texture to a new file
        resultTexture.Save("result_texture.png");
    }*/

    static bool AreTexturesSameSize(Bitmap texture1, Bitmap texture2)
    {
        return texture1.Width == texture2.Width && texture1.Height == texture2.Height;
    }

    static Bitmap GenerateTexture(Bitmap baseTexture, Bitmap overlayTexture)
    {
        // Create a new bitmap to store the generated result
        Bitmap resultTexture = new Bitmap(baseTexture.Width, baseTexture.Height);

        // Loop through each pixel and apply overlay
        for (int x = 0; x < baseTexture.Width; x++)
        {
            for (int y = 0; y < baseTexture.Height; y++)
            {
                Color baseColor = baseTexture.GetPixel(x, y);
                Color overlayColor = overlayTexture.GetPixel(x, y);

                // Modify base color using overlay color (you can customize this blending method)
                Color blendedColor = BlendColors(baseColor, overlayColor);

                // Set the result color to the new blended color
                resultTexture.SetPixel(x, y, blendedColor);
            }
        }

        return resultTexture;
    }

    static Color BlendColors(Color baseColor, Color overlayColor)
    {
        // You can implement your own blending logic here
        // For simplicity, let's just multiply the base color by the overlay color
        int newRed = (int)(baseColor.R * overlayColor.R);
        int newGreen = (int)(baseColor.G * overlayColor.G);
        int newBlue = (int)(baseColor.B * overlayColor.B);

        return Color.FromArgb(newRed, newGreen, newBlue);
    }
}
