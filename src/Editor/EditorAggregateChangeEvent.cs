using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;

namespace UAlbion.Editor;

public class EditorAggregateChangeEvent : Event, IEditorEvent
{
    public EditorAggregateChangeEvent(int id, IList<IEditorEvent> events)
    {
        Id = id;
        Events = events.ToArray();
    }

    public int Id { get; }
    public IReadOnlyList<IEditorEvent> Events { get; }
}