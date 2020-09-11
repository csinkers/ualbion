using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Script)]
    public class ScriptLoader : IAssetLoader
    {
        IEnumerable<string> ReadLines(BinaryReader br, long streamLength)
        {
            var bytes = br.ReadBytes((int)streamLength);
            var text = FormatUtil.BytesTo850String(bytes);
            return text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Select(x => x.Trim());
        }

        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            var events = new List<IEvent>();
            foreach (var line in ReadLines(br, streamLength))
            {
                IEvent e;
                if (string.IsNullOrEmpty(line))
                    e = new CommentEvent(null);
                else if (line.StartsWith(";", StringComparison.Ordinal))
                    e = new CommentEvent(line.Substring(1));
                else
                    e = Event.Parse(line);

                if (e == null)
                {
                    ApiUtil.Assert($"Script line \"{line}\" could not be parsed parsed to an event");
                    e = new UnparsableEvent(line);
                }

                events.Add(e);
            }

            return events;
        }
    }
}
