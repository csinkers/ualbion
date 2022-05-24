using System;
using System.Text;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class JsonLoader<T> : IAssetLoader<T> where T : class
{
    public T Serdes(T existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (s.IsWriting())
        {
            if (existing == null)
                throw new ArgumentNullException(nameof(existing));

            var json = Encoding.UTF8.GetBytes(context.Json.Serialize(existing));
            s.Bytes(null, json, json.Length);
            return existing;
        }
        else
        {
            var json = s.Bytes(null, null, (int) s.BytesRemaining);
            return context.Json.Deserialize<T>(json);
        }
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes((T)existing, info, s, context);
}