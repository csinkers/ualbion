using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
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
        return ApiUtil.SplitLines(text, StringSplitOptions.None).Select(x => x.Trim());
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((Script)existing, s, context);

    public Script Serdes(Script existing, ISerializer s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (s.IsReading())
            return Parse(ReadLines(s));

        ArgumentNullException.ThrowIfNull(existing);

        var builder = new UnformattedScriptBuilder(true);
        foreach (var e in existing)
        {
            e.Format(builder);
            builder.AppendLine();
        }

        var text = builder.Build().TrimEnd() + Environment.NewLine;
        s.FixedLengthString(null, text, text.Length);

        return existing;
    }

    public static Script Parse(string text) => Parse(ApiUtil.SplitLines(text));
    public static Script Parse(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var script = new Script();
        foreach (var line in lines)
        {
            IEvent e;
            if (string.IsNullOrEmpty(line))
                e = new CommentEvent(null);
            else if (line.StartsWith(';'))
                e = new CommentEvent(line[1..]);
            else
            {
                e = Event.Parse(line, out var error);
                if (e == null)
                {
                    ApiUtil.Assert($"Script line \"{line}\" could not be parsed to an event: {error}");
                    e = new UnparsableEvent(line);
                }
            }

            script.Add(e);
        }

        return script;
    }
}