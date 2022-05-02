using UAlbion.Api.Eventing;

namespace UAlbion.Editor;

public interface IEditorEvent : IEvent
{
    int Id { get; }
}