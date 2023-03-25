using System.Collections.Generic;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

public class InspectorPickEvent : Event, IVerboseEvent
{
    public InspectorPickEvent(IList<Selection> selections) => Selections = selections;
    public IList<Selection> Selections { get; }
}