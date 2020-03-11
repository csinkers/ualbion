using System;
using System.Numerics;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class WorldCoordinateSelectEvent : EngineEvent, IVerboseEvent
    {
        readonly Action<float, Selection> _registerHit;

        public WorldCoordinateSelectEvent(Vector3 origin, Vector3 direction, Action<float, Selection> registerHit)
        {
            Origin = origin;
            Direction = direction;
            _registerHit = registerHit;
        }

        public Vector3 Origin { get; }
        public Vector3 Direction { get; }

        public void RegisterHit(float t, object target) => _registerHit(t, new Selection(Origin + t * Direction, target));
    }
}
