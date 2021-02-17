using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers
{
    public class SystemTextLoader : IAssetLoader<IDictionary<int, string>>
    {
        static readonly Regex Regex = new Regex(@"\[(\d+):(.*)\]");

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((IDictionary<int, string>) existing, config, mapping, s);

        public IDictionary<int, string> Serdes(IDictionary<int, string> existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (s.IsWriting()) throw new NotImplementedException("Saving of system text not currently supported");
            var results = new Dictionary<int, string>();
            var bytes = s.ByteArray(null, null, (int)s.BytesRemaining);
            var data = FormatUtil.BytesTo850String(bytes);
            var lines = data.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var m = Regex.Match(line);
                if (!m.Success)
                    continue;

                var subId = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                var text = m.Groups[2].Value;
                results[subId] = text;
            }

            return results;
        }
    }
}
