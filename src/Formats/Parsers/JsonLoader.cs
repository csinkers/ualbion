using System;
using System.Text;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class JsonLoader<T> : IAssetLoader<T> where T : class
{
    public T Serdes(T existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(context);

        if (s.IsWriting())
        {
            ArgumentNullException.ThrowIfNull(existing);

            var jsonText = context.Json.Serialize(existing);
            var json = Encoding.UTF8.GetBytes(jsonText);
            s.Bytes(null, json, json.Length);
            return existing;
        }
        else
        {
            var json = s.Bytes(null, null, (int) s.BytesRemaining);
            return context.Json.Deserialize<T>(json);
        }
    }

    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes((T)existing, s, context);
}