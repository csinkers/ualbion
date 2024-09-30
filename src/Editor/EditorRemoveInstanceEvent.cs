using UAlbion.Api.Eventing;

namespace UAlbion.Editor;

// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable once ClassNeverInstantiated.Global
public record EditorRemoveInstanceEvent(int Id, string CollectionName, int Index, EditorAsset Asset)
    // ReSharper restore NotAccessedPositionalProperty.Global
    : EventRecord, IEditorEvent;
