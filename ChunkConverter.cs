namespace VintageStoryDBToPNG;

public class Coordinates
{
    public int X { get; set; }
    public int Y { get; set; }



    public override string ToString()
    {
        return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}";
    }
}

public class ChunkConverter
{
    public static Coordinates FromChunkIndex(ulong chunkIndex)
    {
        int x = (int)(chunkIndex & ((1UL << 27) - 1));
        int y = (int)(chunkIndex >> 27); // Assuming Y is the higher bits

        return new Coordinates { X = x, Y = y };
    }
    
    public static ulong ToChunkIndex(Coordinates coords) => (ulong) coords.Y << 27 | (ulong) (uint) coords.X;

}