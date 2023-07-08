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
        if (s == null)
            throw new ArgumentNullException(nameof(s));

        if (s.IsReading())
            return Parse(ReadLines(s));

        if (existing == null)
            throw new ArgumentNullException(nameof(existing));

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
        if (lines == null)
            throw new ArgumentNullException(nameof(lines));

        var script = new Script();
        foreach (var line in lines)
        {
            IEvent e;
            if (string.IsNullOrEmpty(line))
                e = new CommentEvent(null);
            else if (line.StartsWith(";", StringComparison.Ordinal))
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