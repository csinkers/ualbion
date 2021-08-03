using System;
using System.Text;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers
{
    public class JsonLoader<T> : IAssetLoader<T> where T : class
    {
        public T Serdes(T existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (s.IsWriting())
            {
                if (existing == null)
                    throw new ArgumentNullException(nameof(existing));

                var json = Encoding.UTF8.GetBytes(JsonUtil.Serialize(existing));
                s.Bytes(null, json, json.Length);
                return existing;
            }
            else
            {
                var json = s.Bytes(null, null, (int) s.BytesRemaining);
                return JsonUtil.Deserialize<T>(json);
            }
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((T)existing, info, mapping, s);
    }
}
