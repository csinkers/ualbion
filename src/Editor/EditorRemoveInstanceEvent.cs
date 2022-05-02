using UAlbion.Api.Eventing;

namespace UAlbion.Editor;

public class EditorRemoveInstanceEvent : Event, IEditorEvent
{
    public int Id { get; }
    public string CollectionName { get; }
    public int Index { get; }
    public EditorAsset Asset { get; }
}