using System;

namespace UAlbion.Core.Events;

public class LogEventArgs : EventArgs
{
    public DateTime Time { get; set; }
    public int Nesting { get; set; }
    public ConsoleColor Color { get; set; }
    public string Message { get; set; }
}