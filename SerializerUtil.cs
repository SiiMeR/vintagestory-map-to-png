using ProtoBuf;

namespace VintageStoryDBToPNG;


public static class SerializerUtil
{
    public static T Deserialize<T>(byte[] data)
    {
        using (MemoryStream memoryStream = new MemoryStream(data))
            return Serializer.Deserialize<T>((Stream) memoryStream);
    }
}