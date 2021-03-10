using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Parsers
{
    public class ScriptLoader : IAssetLoader<IList<IEvent>>
    {
        static IEnumerable<string> ReadLines(ISerializer s)
        {
            var bytes = s.ByteArray(null, null, (int)s.BytesRemaining);
            var text = FormatUtil.BytesTo850String(bytes);
            return text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Select(x => x.Trim());
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((IList<IEvent>)existing, info, mapping, s);

        public IList<IEvent> Serdes(IList<IEvent> events, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            if (s.IsReading())
            {
                events = new List<IEvent>();
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
                        ApiUtil.Assert($"Script line \"{line}\" could not be parsed to an event");
                        e = new UnparsableEvent(line);
                    }

                    events.Add(e);
                }
            }
            else
            {
                if (events == null) throw new ArgumentNullException(nameof(events));
                var sb = new StringBuilder();
                foreach (var e in events)
                    sb.AppendLine(e.ToString());
                s.NullTerminatedString(null, sb.ToString());
            }

            return events;
        }
    }
}
