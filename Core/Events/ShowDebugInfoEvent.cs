using System.Collections.Generic;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class ShowDebugInfoEvent : Event, IVerboseEvent
    {
        public ShowDebugInfoEvent(IList<Selection> selections)
        {
            Selections = selections;
        }

        public IList<Selection> Selections { get; }
    }
}