using System.Text.Json;

namespace VintageStoryDBToPNG;

public class Config
{
    public string MapFile { get; set; }
    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int MinY { get; set; }
    public int MaxY { get; set; }
}

public static class ConfigHelpers
{
    public static Config ReadConfig(string fileName)
    {
        try
        {
            string configFile = fileName; // Adjust the path if needed
            string jsonContent = File.ReadAllText(configFile);

            // Deserialize the JSON content into a Config object
            var config = JsonSerializer.Deserialize<Config>(jsonContent);

            if (config == null)
            {
                throw new Exception($"No {fileName} found!");
            }
            
            return config;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading configuration: {ex.Message}");
            throw;
        }
    }

}