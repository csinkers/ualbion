using System;

namespace UAlbion.Api.Eventing;

public class LogEventArgs : EventArgs
{
    public DateTime Time { get; set; }
    public int Nesting { get; set; }
    public ConsoleColor Color { get; set; }
    public string Message { get; set; }
}