using UAlbion.Api.Eventing;

namespace UAlbion.Editor;

// ReSharper disable once ClassNeverInstantiated.Global
public record EditorAddInstanceEvent(int Id, string CollectionName, int Index) : EventRecord, IEditorEvent;