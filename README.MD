# Vintage Story Map Exporter

This is a tool to read the color values of your Vintage Story client-side map file
from a .db file and export them to a .png file. 

## How to use

1. Fill in the values in config.json
   * MapFile - The path to your map .db file (usually found in VintageStoryData/Maps). Warning: Make a backup of your map before using this program. 
   * The bounding box of the exported map. Note that these are absolute coordinates (you can find your position ingame by writing `.cp aposi` to the chat). From there you will have to guesstimate the bounds:
     * MinX - The leftmost coordinate the map will be exported from
     * MaxX - The rightmost coordinate the map will be exported to
     * MinY - The uppermost coordinate the map will be exported from (in VS, the Y coordinates are flipped)
     * MaxY - The lowermost coordinate the map will be export to

2. Run the provided `VintageStoryDBToPNG.exe` file
3. The map will be exported to the same directory

**The map colors are from the [Medieval Map](https://mods.vintagestory.at/medievalmap) mod**

## Limitations

* The max size of the exported map is limited due to the Bitmap class in C#. You can export about 10k x 10k map at a time. Or you could implement stitching of smaller map pieces yourself. 
