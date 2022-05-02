namespace UAlbion.Api.Eventing;

[Event("log", "Writes to the game log.")]
public class LogEvent : Event
{
    [EventPart("severity", "The severity of the log event (0=Verbose, 1=Info, 2=Warning, 3=Error, 4=Critical)")]
    public LogLevel Severity { get; }

    [EventPart("msg", "The log message")]
    public string Message { get; }

    [EventPart("file", "The source file that emitted the message", true)] public string File { get; }
    [EventPart("method", "The member that emitted the message", true)] public string Member { get; }
    [EventPart("line", "The source line that emitted the message", true)] public int? Line { get; }

    public LogEvent(LogLevel severity, string message, string file = null, string member = null, int? line = null)
    {
        Severity = severity;
        Message = message;
        File = file;
        Member = member;
        Line = line;
    }
}