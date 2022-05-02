using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Editor;

public class EditorMoveInstanceEvent : Event, IEditorEvent
{
    public int Id { get; }
    public string CollectionName { get; }
    public int FromIndex { get; }
    public int ToIndex { get; }
}