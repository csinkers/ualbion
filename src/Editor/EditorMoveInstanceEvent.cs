using UAlbion.Api.Eventing;

namespace UAlbion.Editor;

// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable once ClassNeverInstantiated.Global
public record EditorMoveInstanceEvent(int Id, string CollectionName, int FromIndex, int ToIndex) : EventRecord, IEditorEvent;
// ReSharper restore NotAccessedPositionalProperty.Global
