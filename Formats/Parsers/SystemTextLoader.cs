using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.SystemText)]
    public class SystemTextLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            var regex = new Regex(@"\[(\d+):(.*)\]");
            var results = new Dictionary<int, string>();
            var bytes = br.ReadBytes((int)streamLength);
            var data = StringUtils.BytesTo850String(bytes);
            var lines = data.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var m = regex.Match(line);
                if (!m.Success)
                    continue;

                var id = int.Parse(m.Groups[1].Value);
                var text = m.Groups[2].Value;
                results[id] = text;
            }

            return results;
        }
    }
}
