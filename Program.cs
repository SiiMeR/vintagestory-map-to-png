using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;

namespace VintageStoryDBToPNG
{
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

            // Create and save tiled images
            var tilePaths = CreateAndSaveTiledImages(mapPieces, chunkWidth, chunkHeight, out int finalWidth, out int finalHeight);

            // Stitch tiles together
            StitchTiles(tilePaths, finalWidth, finalHeight);

            Console.WriteLine("Success");
        }

        static List<string> CreateAndSaveTiledImages(List<MapPiece> mapPieces, int chunkWidth, int chunkHeight, out int finalWidth, out int finalHeight)
        {
            int tileSize = 8192; // Define the tile size (e.g., 8192x8192 pixels)
            int totalChunks = mapPieces.Count;

            // Find the minimum X and Y coordinates
            int minX = mapPieces.Min(piece => piece.Coordinates.X);
            int minY = mapPieces.Min(piece => piece.Coordinates.Y);

            // Calculate the normalized width and height of the combined image
            finalWidth = (mapPieces.Max(piece => piece.Coordinates.X) - minX + 1) * chunkWidth;
            finalHeight = (mapPieces.Max(piece => piece.Coordinates.Y) - minY + 1) * chunkHeight;

            int horizontalTiles = (int)Math.Ceiling((double)finalWidth / tileSize);
            int verticalTiles = (int)Math.Ceiling((double)finalHeight / tileSize);

            List<string> tilePaths = new List<string>();

            for (int tileY = 0; tileY < verticalTiles; tileY++)
            {
                for (int tileX = 0; tileX < horizontalTiles; tileX++)
                {
                    int startX = tileX * tileSize;
                    int startY = tileY * tileSize;
                    int endX = Math.Min(startX + tileSize, finalWidth);
                    int endY = Math.Min(startY + tileSize, finalHeight);

                    Console.WriteLine($"Creating tile ({tileX}, {tileY}) with bounds ({startX}, {startY}) to ({endX}, {endY})");

                    Bitmap tileImage = new Bitmap(tileSize, tileSize);

                    using Graphics g = Graphics.FromImage(tileImage);
                    g.Clear(Color.White); // Fill the background with white

                    foreach (var mapPiece in mapPieces)
                    {
                        int pieceX = (mapPiece.Coordinates.X - minX) * chunkWidth;
                        int pieceY = (mapPiece.Coordinates.Y - minY) * chunkHeight;

                        if (pieceX < endX && pieceX + chunkWidth > startX && pieceY < endY && pieceY + chunkHeight > startY)
                        {
                            for (int y = 0; y < chunkHeight; y++)
                            {
                                for (int x = 0; x < chunkWidth; x++)
                                {
                                    int normalizedX = pieceX + x - startX;
                                    int normalizedY = pieceY + y - startY;

                                    if (normalizedX >= 0 && normalizedX < tileSize && normalizedY >= 0 && normalizedY < tileSize)
                                    {
                                        int pixelIndex = y * chunkWidth + x;
                                        var color = PixelColorConverter.ConvertToColor(mapPiece.Colors.Pixels[pixelIndex]);

                                        tileImage.SetPixel(normalizedX, normalizedY, color);
                                    }
                                }
                            }
                        }
                    }

                    string tileFileName = $"map_tile_{tileX}_{tileY}.png";
                    tilePaths.Add(tileFileName);
                    Console.WriteLine($"Saving tile to {tileFileName}");
                    tileImage.Save(tileFileName, ImageFormat.Png);
                }
            }

            return tilePaths;
        }

        static void StitchTiles(List<string> tilePaths, int finalWidth, int finalHeight)
        {
            Bitmap finalImage = new Bitmap(finalWidth, finalHeight);

            int tileSize = 8192;

            using (Graphics g = Graphics.FromImage(finalImage))
            {
                foreach (var tilePath in tilePaths)
                {
                    string[] parts = tilePath.Split(new[] { '_', '.' }, StringSplitOptions.RemoveEmptyEntries);
                    int tileX = int.Parse(parts[2]);
                    int tileY = int.Parse(parts[3]);

                    using (Bitmap tileImage = new Bitmap(tilePath))
                    {
                        g.DrawImage(tileImage, tileX * tileSize, tileY * tileSize);
                    }
                }
            }

            finalImage.Save("stitched_map.png", ImageFormat.Png);
        }
    }
}
