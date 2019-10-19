using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Input
{
    public class DebugPickMouseMode : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<DebugPickMouseMode, InputEvent>((x,e) => x.OnInput(e))
        );

        void OnInput(InputEvent e)
        {
            if(e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && x.Down))
            {
                Raise(new PopMouseModeEvent());
                return;
            }

            IList<(float, Selection)> hits = new List<(float, Selection)>();
            Raise(new ScreenCoordinateSelectEvent(e.Snapshot.MousePosition, (t, selection) => hits.Add((t, selection))));
            var orderedHits = hits.OrderBy(x => x.Item1).Select(x => x.Item2).ToList();
            Raise(new ShowDebugInfoEvent(orderedHits, e.Snapshot.MousePosition));
        }

        public DebugPickMouseMode() : base(Handlers) { }
    }
}