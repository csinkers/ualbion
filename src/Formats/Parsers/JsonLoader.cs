using System;
using System.Text;
using Newtonsoft.Json;
using SerdesNet;
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

                var json = JsonConvert.SerializeObject(existing, ConfigUtil.JsonSerializerSettings);
                var bytes = Encoding.UTF8.GetBytes(json);
                s.ByteArray(null, bytes, bytes.Length);
                return existing;
            }
            else
            {
                var bytes = s.ByteArray(null, null, (int) s.BytesRemaining);
                var json = Encoding.UTF8.GetString(bytes);
                return (T)JsonConvert.DeserializeObject<T>(json, ConfigUtil.JsonSerializerSettings);
            }
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((T)existing, info, mapping, s);
    }
}
