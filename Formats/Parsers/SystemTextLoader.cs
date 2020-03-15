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
        static readonly Regex Regex = new Regex(@"\[(\d+):(.*)\]");
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            var results = new Dictionary<int, string>();
            var bytes = br.ReadBytes((int)streamLength);
            var data = StringUtils.BytesTo850String(bytes);
            var lines = data.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var m = Regex.Match(line);
                if (!m.Success)
                    continue;

                var id = int.Parse(m.Groups[1].Value);
                var text = m.Groups[2].Value;
                results[id] = text;
            }

            return results;
/*
            var fullText = FormatUtil.BytesTo850String(br.ReadBytes((int)streamLength));
            var lines = fullText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line[0] != '[')
                    continue;
                var untilColon = line.Substring(1, line.IndexOf(':') - 1);
                int id = int.Parse(untilColon);
                strings[id] = line.Substring(line.IndexOf(':') + 1).TrimEnd(']');
            }
*/
        }
    }
}
