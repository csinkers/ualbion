using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Editor
{
    public class EditorAggregateChangeEvent : IEditorEvent
    {
        public EditorAggregateChangeEvent(int id, IList<IEditorEvent> events)
        {
            Id = id;
            Events = events.ToArray();
        }

        public int Id { get; }
        public IReadOnlyList<IEditorEvent> Events { get; }
    }
}