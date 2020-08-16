using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Script)]
    public class ScriptLoader : IAssetLoader
    {
        IEnumerable<string> ReadLines(BinaryReader br, long streamLength)
        {
            var remaining = streamLength;
            var sb = new StringBuilder();
            bool inComment = false;
            while (remaining > 0)
            {
                char c = br.ReadChar();
                remaining--;

                if(c == '\n' || c == '\r')
                {
                    if (sb.Length > 0)
                    {
                        var s = sb.ToString().Trim();
                        if (s.Length > 0)
                            yield return s;
                        sb.Clear();
                    }
                    inComment = false;
                }
                else if (c == ';')
                {
                    inComment = true;
                }
                else if (!inComment)
                {
                    sb.Append(c);
                }
            }

            if (sb.Length > 0) // Handle final line
            {
                var s = sb.ToString().Trim();
                if (s.Length > 0)
                    yield return s;
                sb.Clear();
            }
        }

        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            var events = new List<IEvent>();
            foreach (var line in ReadLines(br, streamLength))
            {
                var e = Event.Parse(line);
                if (e == null)
                    ApiUtil.Assert($"Script line \"{line}\" could not be parsed parsed to an event");
                else
                    events.Add(e);
            }

            return events;
        }
    }
}
