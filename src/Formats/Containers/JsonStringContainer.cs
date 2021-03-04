using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    /// <summary>
    /// Container encoding a list of strings to a JSON object/dictionary
    /// </summary>
    public class JsonStringContainer : IAssetContainer
    {
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The serializer will handle it")]
        public ISerializer Read(string path, AssetInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            var dict = Load(path);
            if (!dict.TryGetValue(info.AssetId, out var token))
                return null;

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(token, ConfigUtil.JsonSerializerSettings)));
            var br = new BinaryReader(ms);
            return new GenericBinaryReader(
                br,
                ms.Length,
                Encoding.UTF8.GetString,
                ApiUtil.Assert,
                () => { br.Dispose(); ms.Dispose(); });
        }

        public void Write(string path, IList<(AssetInfo, byte[])> assets)
        {
            if (assets == null) throw new ArgumentNullException(nameof(assets));
            var dict = new Dictionary<string, string>();
            foreach(var (info, bytes) in assets)
                dict[info.AssetId.ToString()] = Encoding.UTF8.GetString(bytes);

            var fullText = JsonConvert.SerializeObject(dict, ConfigUtil.JsonSerializerSettings);
            File.WriteAllText(path, fullText);
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info)
        {
            if (!File.Exists(path))
                return null;
            var dict = Load(path);
            return FormatUtil.SortedIntsToRanges(dict.Keys.Select(x => x.Id).OrderBy(x => x));
        }

        static IDictionary<AssetId, string> Load(string path)
        {
            var text = File.ReadAllText(path);
            var dict = (IDictionary<string, string>)JsonConvert.DeserializeObject<IDictionary<string, string>>(text);
            if (dict == null)
                throw new FileLoadException($"Could not deserialize \"{path}\"");

            return dict.ToDictionary(x => AssetId.Parse(x.Key), x => x.Value);
        }
    }
}