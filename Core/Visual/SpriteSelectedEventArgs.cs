using System;
using UAlbion.Core.Events;

namespace UAlbion.Core.Visual
{
    public class SpriteSelectedEventArgs : EventArgs
    {
        public float HitPosition { get; }
        public WorldCoordinateSelectEvent SelectEvent { get; }
        public bool Handled { get; set; }

        public SpriteSelectedEventArgs(float hitPosition, WorldCoordinateSelectEvent selectEvent)
        {
            HitPosition = hitPosition;
            SelectEvent = selectEvent;
        }
    }
}
