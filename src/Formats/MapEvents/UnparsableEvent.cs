using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public class UnparsableEvent : IVerboseEvent // No-op event for parsing failures in script files.
    {
        public UnparsableEvent(string rawEventText) => RawEventText = rawEventText;
        public string RawEventText { get; }
        public override string ToString() => RawEventText;
        public string ToStringNumeric() => ToString();
    }
}
