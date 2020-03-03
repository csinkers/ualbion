namespace UAlbion.Api
{
    [Event("set_log_level")]
    public class SetLogLevelEvent : Event, IVerboseEvent
    {
        public SetLogLevelEvent(LogEvent.Level level)
        {
            Level = level;
        }

        [EventPart("level")]
        public LogEvent.Level Level { get; }
    }
}