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
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<DebugPickMouseMode, SetMouseModeEvent>((x,e) => x._isActive = e.Mode == MouseModeId.DebugPick),
            new Handler<DebugPickMouseMode, InputEvent>((x,e) => x.OnInput(e)), 
        };

        bool _isActive;

        void OnInput(InputEvent e)
        {
            if (!_isActive)
                return;

            if(e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && x.Down))
            {
                Raise(new SetMouseModeEvent((int)MouseModeId.Normal));
                return;
            }

            IList<(float, Selection)> hits = new List<(float, Selection)>();
            Raise(new ScreenCoordinateSelectEvent(e.Snapshot.MousePosition, (t, selection) => hits.Add((t, selection))));
            var orderedHits = hits.OrderBy(x => x.Item1).Select(x => x.Item2).ToList();
            Raise(new ShowDebugInfoEvent(orderedHits));
        }

        public DebugPickMouseMode() : base(Handlers) { }
    }
}