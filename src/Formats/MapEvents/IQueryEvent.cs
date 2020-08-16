using System;
namespace UAlbion.Formats.MapEvents
{
    // Events of size 8 stored in nodes with a FalseEventId
    public interface IQueryEvent : IBranchingEvent
    {
        QueryType QueryType { get; }
    }
}
