using System;
using System.Globalization;
using System.Text.RegularExpressions;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class SystemTextLoader : IAssetLoader<IntStringDictionary>
    {
        static readonly Regex Regex = new(@"\[(\d+):(.*)\]");

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((IntStringDictionary) existing, info, mapping, s);

        public IntStringDictionary Serdes(IntStringDictionary existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (s.IsWriting()) throw new NotImplementedException("Saving of system text not currently supported");
            var results = new IntStringDictionary();
            var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
            var data = FormatUtil.BytesTo850String(bytes);
            foreach (var line in FormatUtil.SplitLines(data))
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
