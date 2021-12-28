namespace UAlbion.Api;

[Event("set_log_level")]
public class SetLogLevelEvent : Event, IVerboseEvent
{
    public SetLogLevelEvent(LogLevel level) => Level = level;
    [EventPart("level")] public LogLevel Level { get; }
}