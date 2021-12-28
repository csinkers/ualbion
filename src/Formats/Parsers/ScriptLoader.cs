using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Parsers;

public class ScriptLoader : IAssetLoader<Script>
{
    static IEnumerable<string> ReadLines(ISerializer s)
    {
        var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
        var text = FormatUtil.BytesTo850String(bytes);
        return text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Select(x => x.Trim());
    }

    public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => Serdes((Script)existing, info, mapping, s, jsonUtil);

    public Script Serdes(Script script, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        if (s.IsReading())
        {
            script = new Script();
            foreach (var line in ReadLines(s))
            {
                IEvent e;
                if (string.IsNullOrEmpty(line))
                    e = new CommentEvent(null);
                else if (line.StartsWith(";", StringComparison.Ordinal))
                    e = new CommentEvent(line[1..]);
                else
                    e = Event.Parse(line);

                if (e == null)
                {
                    ApiUtil.Assert($"Script line \"{line}\" could not be parsed to an event");
                    e = new UnparsableEvent(line);
                }

                script.Add(e);
            }
        }
        else
        {
            if (script == null) throw new ArgumentNullException(nameof(script));
            var sb = new StringBuilder();
            foreach (var e in script)
                sb.AppendLine(e.ToStringNumeric());

            var text = sb.ToString();
            s.FixedLengthString(null, text, text.Length);
        }

        return script;
    }
}