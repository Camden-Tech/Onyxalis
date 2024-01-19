using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

public class TextureGenerator
{
    public static bool AreTexturesSameSize(Texture2D texture1, Texture2D texture2)
    {
        return texture1.Width == texture2.Width && texture1.Height == texture2.Height;
    }

    public static Texture2D GenerateTexture(Texture2D baseTexture, Texture2D overlayTexture, GraphicsDevice device)
    {
        if (!AreTexturesSameSize(baseTexture, overlayTexture))
        {
            return null;
        }

        // Using byte arrays to handle texture data
        byte[] baseTextureData = new byte[baseTexture.Width * baseTexture.Height * 4];
        byte[] overlayTextureData = new byte[overlayTexture.Width * overlayTexture.Height * 4];

        baseTexture.GetData<byte>(baseTextureData);
        overlayTexture.GetData<byte>(overlayTextureData);

        byte[] resultData = new byte[baseTextureData.Length];

        // Loop through each pixel and apply overlay
        for (int i = 0; i < baseTextureData.Length; i += 4)
        {
            // Extracting color components from byte array
            Color baseColor = new Color(baseTextureData[i + 0], baseTextureData[i + 1], baseTextureData[i + 2], baseTextureData[i + 3]);
            Color overlayColor = new Color(overlayTextureData[i + 0], overlayTextureData[i + 1], overlayTextureData[i + 2], overlayTextureData[i + 3]);

            // Modify base color using overlay color
            Color blendedColor = BlendColors(baseColor, overlayColor);

            // Repacking color components into byte array
            resultData[i + 0] = blendedColor.R;
            resultData[i + 1] = blendedColor.G;
            resultData[i + 2] = blendedColor.B;
            resultData[i + 3] = blendedColor.A; // Handle the alpha channel if necessary
        }

        // Create a new Texture2D and set the data
        Texture2D resultTexture = new Texture2D(device, baseTexture.Width, baseTexture.Height);
        resultTexture.SetData<byte>(resultData);

        return resultTexture;
    }

    public static Color BlendColors(Color baseColor, Color overlayColor)
    {
        // Implement your blending logic here
        int newRed = (baseColor.R - overlayColor.R) * (255 - overlayColor.A) / 255 + overlayColor.R;
        int newGreen = (baseColor.G - overlayColor.G) * (255 - overlayColor.A) / 255 + overlayColor.G;
        int newBlue = (baseColor.B - overlayColor.B) * (255 - overlayColor.A) / 255 + overlayColor.B;

        return new Color(newRed, newGreen, newBlue);
    }

    public static void SaveTexture(Texture2D texture, string fileName)
    {
        using (FileStream stream = new FileStream(fileName + ".png", FileMode.Create))
        {
            texture.SaveAsPng(stream, texture.Width, texture.Height);
        }
    }
    public static Texture2D[,] BreakIntoPieces(Texture2D originalTexture, GraphicsDevice graphicsDevice)
    {
        // Size of each piece
        int pieceWidth = 8;
        int pieceHeight = 8;

        // Calculate the number of pieces along each dimension
        int piecesX = originalTexture.Width / pieceWidth;
        int piecesY = originalTexture.Height / pieceHeight;

        // Initialize the 2D array of textures
        Texture2D[,] pieces = new Texture2D[piecesX, piecesY];

        // Extract each piece
        for (int x = 0; x < piecesX; x++)
        {
            for (int y = 0; y < piecesY; y++)
            {
                // Create a new texture for the piece
                Texture2D pieceTexture = new Texture2D(graphicsDevice, pieceWidth, pieceHeight);

                // Copy the data from the original texture to the piece
                Color[] data = new Color[pieceWidth * pieceHeight];
                originalTexture.GetData(0, new Rectangle(x * pieceWidth, y * pieceHeight, pieceWidth, pieceHeight), data, 0, data.Length);
                pieceTexture.SetData(data);

                // Store the piece in the array
                pieces[x, y] = pieceTexture;
            }
        }

        return pieces;
    }



}