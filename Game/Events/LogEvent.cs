using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("log", "Writes to the game log.")]
    public class LogEvent : GameEvent
    {
        public enum Level
        {
            Verbose,
            Info,
            Warning,
            Error,
            Critical
        }

        [EventPart("severity", "The severity of the log event (0=Verbose, 1=Info, 2=Warning, 3=Error, 4=Critical)")]
        public Level Severity { get; }

        [EventPart("msg", "The log message")]
        public string Message { get; }

        public LogEvent(Level severity, string message)
        {
            Severity = severity;
            Message = message;
        }
    }
}