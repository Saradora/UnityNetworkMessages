using OdinSerializer;
using OdinSerializer.Utilities;

namespace UnityNetMessages.OdinSerializer;

public static class Serialization
{
    public static T Deserialize<T>(byte[] bytes) => (T)Deserialize(bytes, typeof(T));
    
    public static object Deserialize(byte[] bytes, Type type)
    {
        using Cache<CachedMemoryStream> cache = CachedMemoryStream.Claim(bytes);
        
        Cache<BinaryDataReader> cache2 = Cache<BinaryDataReader>.Claim();
        BinaryDataReader binaryDataReader = cache2.Value;
        binaryDataReader.Stream = cache.Value.MemoryStream;
        binaryDataReader.Context = null;
        binaryDataReader.PrepareNewSerializationSession();
        IDataReader cachedReader = binaryDataReader;
        IDisposable cache1 = cache2;
        try
        {
            using Cache<DeserializationContext> deseCache = Cache<DeserializationContext>.Claim();
                
            cachedReader.Context = deseCache;
            return Serializer.Get(type).ReadValueWeak(cachedReader);
        }
        finally
        {
            cache1.Dispose();
        }
    }
}