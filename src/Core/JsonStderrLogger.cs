using System;
using System.Text.Json;
using UAlbion.Api.Eventing;

namespace UAlbion.Core;

public class JsonStderrLogger : Component
{
    protected override void Subscribed()
    {
        var logExchange = Resolve<ILogExchange>();
        logExchange.Log += Write;
    }

    protected override void Unsubscribed()
    {
        var logExchange = Resolve<ILogExchange>();
        logExchange.Log -= Write;
    }

    static void Write(object sender, LogEventArgs log)
    {
        var sev = log.Color switch
        {
            ConsoleColor.Red    => "Error",
            ConsoleColor.Yellow => "Warn",
            _                   => "Info",
        };

        var line = JsonSerializer.Serialize(new
        {
            t   = log.Time.ToString("HH:mm:ss.fff"),
            n   = log.Nesting,
            sev,
            msg = log.Message,
        });

        Console.Error.WriteLine(line);
    }
}
