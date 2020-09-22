using UAlbion.Api;

namespace UAlbion.Editor
{
    public interface IEditorEvent : IEvent
    {
        int Id { get; }
    }
}