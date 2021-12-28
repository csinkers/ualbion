using System;
using System.Text;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class JsonLoader<T> : IAssetLoader<T> where T : class
{
    public T Serdes(T existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));

        if (s.IsWriting())
        {
            if (existing == null)
                throw new ArgumentNullException(nameof(existing));

            var json = Encoding.UTF8.GetBytes(jsonUtil.Serialize(existing));
            s.Bytes(null, json, json.Length);
            return existing;
        }
        else
        {
            var json = s.Bytes(null, null, (int) s.BytesRemaining);
            return jsonUtil.Deserialize<T>(json);
        }
    }

    public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => Serdes((T)existing, info, mapping, s, jsonUtil);
}