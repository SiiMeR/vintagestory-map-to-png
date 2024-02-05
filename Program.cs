#pragma warning disable CA1416
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;

namespace VintageStoryDBToPNG;

static class Program
{
    static void Main()
    {
        var config = ConfigHelpers.ReadConfig("config.json");
        CreateMapWithBounds(config.MinX, config.MaxX, config.MinY, config.MaxY, config.MapFile);
    }

    static void CreateMapWithBounds(int minX, int maxX, int minY, int maxY, string mapFileName)
    {
        if (!File.Exists(mapFileName))
        {
            throw new Exception($"No map file found at {mapFileName}");
        }
        
        // Assuming each chunk has 32x32 pixels
        int chunkWidth = 32;
        int chunkHeight = 32;

        string dbFilePath = Path.IsPathRooted(mapFileName) ? mapFileName : Path.Combine(Directory.GetCurrentDirectory(), mapFileName);

        using SQLiteConnection connection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;");
        connection.Open();

        string query = "SELECT * FROM mappiece";

        using SQLiteCommand command = new SQLiteCommand(query, connection);
        using SQLiteDataReader reader = command.ExecuteReader();

        List<MapPiece> mapPieces = new List<MapPiece>();

        Console.WriteLine("Reading map DB...");
        while (reader.Read())
        {
            var position = Convert.ToUInt64(reader["position"]);
            byte[] data = (byte[])reader["data"];

            var mapPiece = SerializerUtil.Deserialize<MapPieceColors>(data);

            var coordinates = ChunkConverter.FromChunkIndex(position);

            if (coordinates.X * chunkWidth >= minX && coordinates.X * chunkWidth <= maxX &&
                coordinates.Y * chunkHeight >= minY && coordinates.Y * chunkHeight <= maxY)
            {
                mapPieces.Add(new MapPiece(mapPiece, coordinates));
            }
        }

        Console.WriteLine($"Loaded {mapPieces.Count} chunks from map DB");
        /*
    Image image = CreateImageFromColors(mapPieces[0].Colors.Pixels, 32, 32);
    File.Delete($"chunk_{reader["position"]}.png");
    string imagePath = $"chunk_{reader["position"]}.png";
    image.Save(imagePath, ImageFormat.Png);

    */


        // Create an image with all chunks
        Image combinedImage = CreateCombinedImage(mapPieces, chunkWidth, chunkHeight);

        var fileName = $"{Path.GetFileNameWithoutExtension(mapFileName)}_exported.png";
        Console.WriteLine($"Exporting map to {fileName}");
        string combinedImagePath = fileName;
        combinedImage.Save(combinedImagePath, ImageFormat.Png);

        Console.WriteLine("Success");
    }


    static Image CreateCombinedImage(List<MapPiece> mapPieces, int chunkWidth, int chunkHeight)
    {
        int totalChunks = mapPieces.Count;

        // Find the minimum X and Y coordinates
        int minX = mapPieces.Min(piece => piece.Coordinates.X);
        int minY = mapPieces.Min(piece => piece.Coordinates.Y);

        // Calculate the normalized width and height of the combined image
        int totalWidth = (mapPieces.Max(piece => piece.Coordinates.X) - minX + 1) * chunkWidth;
        int totalHeight = (mapPieces.Max(piece => piece.Coordinates.Y) - minY + 1) * chunkHeight;

        Console.WriteLine($"Creating image with width {totalWidth} and height {totalHeight}");

        Bitmap combinedImage = new Bitmap(totalWidth, totalHeight);


        using Graphics g = Graphics.FromImage(combinedImage);
        for (int i = 0; i < totalChunks; i++)
        {
            var mapPiece = mapPieces[i];

            for (int y = 0; y < chunkHeight; y++)
            {
                for (int x = 0; x < chunkWidth; x++)
                {
                    int normalizedX = (mapPiece.Coordinates.X - minX) * chunkWidth + x;
                    int normalizedY = (mapPiece.Coordinates.Y - minY) * chunkHeight + y;

                    int pixelIndex = y * chunkWidth + x;
                    var color = PixelColorConverter.ConvertToColor(mapPiece.Colors.Pixels[pixelIndex]);

                    // Set pixel in the combined image
                    combinedImage.SetPixel(normalizedX, normalizedY, color);
                }
            }
        }

        return combinedImage;
    }

    static Image CreateImageFromColors(int[] colors, int width, int height)
    {
        Bitmap image = new Bitmap(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                Color color = PixelColorConverter.ConvertToColor(colors[index]);
                image.SetPixel(x, y, color);
            }
        }

        return image;
    }
}