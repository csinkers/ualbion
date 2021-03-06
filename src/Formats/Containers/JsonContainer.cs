﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    /// <summary>
    /// Container encoding a list of assets to a JSON object/dictionary
    /// </summary>
    public class JsonObjectContainer : IAssetContainer
    {
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The serializer will handle it")]
        public ISerializer Read(string path, AssetInfo info, IFileSystem disk)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            if (!disk.FileExists(path))
                return null;

            var dict = Load(path, disk);
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

        public void Write(string path, IList<(AssetInfo, byte[])> assets, IFileSystem disk)
        {
            if (assets == null) throw new ArgumentNullException(nameof(assets));
            if (disk == null) throw new ArgumentNullException(nameof(disk));

            var dir = Path.GetDirectoryName(path);
            if (!disk.DirectoryExists(dir))
                disk.CreateDirectory(dir);

            var dict = new Dictionary<string, JObject>();
            foreach (var (info, bytes) in assets)
            {
                var json = Encoding.UTF8.GetString(bytes);
                var jObject = JObject.Parse(json);
                dict[info.AssetId.ToString()] = jObject;
            }

            var fullText = JsonConvert.SerializeObject(dict, ConfigUtil.JsonSerializerSettings);
            disk.WriteAllText(path, fullText);
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            if (!disk.FileExists(path))
                return null;
            var dict = Load(path, disk);
            return FormatUtil.SortedIntsToRanges(dict.Keys.Select(x => x.Id).OrderBy(x => x));
        }

        static IDictionary<AssetId, JObject> Load(string path, IFileSystem disk)
        {
            var text = disk.ReadAllText(path);
            var dict = (IDictionary<string, JObject>)JsonConvert.DeserializeObject<IDictionary<string, JObject>>(text);
            if (dict == null)
                throw new FileLoadException($"Could not deserialize \"{path}\"");

            return dict.ToDictionary(x => AssetId.Parse(x.Key), x => x.Value);
        }
    }
}
