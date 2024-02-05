namespace VintageStoryDBToPNG;

public record MapPieceColors(int[] Pixels);
public record MapPiece(MapPieceColors Colors, Coordinates Coordinates);
