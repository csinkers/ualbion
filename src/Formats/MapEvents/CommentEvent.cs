using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public class CommentEvent : IVerboseEvent // No-op event for preserving comments in script files
    {
        [EventPart("msg")] public string Comment { get; }
        public CommentEvent(string comment) => Comment = comment;
        public override string ToString() => Comment == null ? "" : $";{Comment}";
    }
}
