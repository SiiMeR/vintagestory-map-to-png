using System.Drawing;

namespace VintageStoryDBToPNG;

public static class PixelColorConverter
{
    public static Color ConvertToColor(int pixelValue)
    {
        // Extract RGB components from the pixel value
        int blue = (pixelValue >> 16) & 0xFF;
        int green = (pixelValue >> 8) & 0xFF;
        int red = pixelValue & 0xFF;

        // Create a Color object using the RGB components
        Color color = Color.FromArgb( red, green, blue);
        return color;
    }
    
    public static Color[] ConvertPixelsToColors(int[] pixels)
    {
        Color[] colors = new Color[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            colors[i] = ConvertToColor(pixels[i]);
        }

        return colors;
    }
}
