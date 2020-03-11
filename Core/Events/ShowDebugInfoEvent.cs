using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class ShowDebugInfoEvent : Event, IVerboseEvent
    {
        public ShowDebugInfoEvent(IList<Selection> selections, Vector2 mousePosition)
        {
            Selections = selections;
            MousePosition = mousePosition;
        }

        public IList<Selection> Selections { get; }
        public Vector2 MousePosition { get; }
    }
}
