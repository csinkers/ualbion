using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Parsers
{
    public class ScriptLoader : IAssetLoader
    {
        IEnumerable<string> ReadLines(ISerializer s)
        {
            var bytes = s.ByteArray(null, null, (int)s.BytesRemaining);
            var text = FormatUtil.BytesTo850String(bytes);
            return text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Select(x => x.Trim());
        }

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var events = new List<IEvent>();
            foreach (var line in ReadLines(s))
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
