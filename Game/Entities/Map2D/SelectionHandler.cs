using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Entities.Map2D
{
    public sealed class SelectionHandler : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<SelectionHandler, WorldCoordinateSelectEvent>((x, e) => x.OnSelect(e))
        );

        static readonly Vector3 Normal = Vector3.UnitZ;
        readonly LogicalMap _map;
        readonly Vector2 _tileSize;
        int _lastHighlightIndex;

        public SelectionHandler(LogicalMap map, Vector2 tileSize) : base(Handlers)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _tileSize = tileSize;
        }

        public event EventHandler<int> HighlightIndexChanged;

        void OnSelect(WorldCoordinateSelectEvent e)
        {
            float denominator = Vector3.Dot(Normal, e.Direction);
            if (Math.Abs(denominator) < 0.00001f)
                return;

            float t = Vector3.Dot(-e.Origin, Normal) / denominator;
            if (t < 0)
                return;

            Vector3 intersectionPoint = e.Origin + t * e.Direction;
            int x = (int)(intersectionPoint.X / _tileSize.X);
            int y = (int)(intersectionPoint.Y / _tileSize.Y);

            int highlightIndex = y * _map.Width + x;
            var underlayTile = _map.GetUnderlay(x, y);
            var overlayTile = _map.GetOverlay(x, y);
            var zone = _map.GetZone(x, y);

            e.RegisterHit(t, new MapTileHit(new Vector2(x, y), intersectionPoint));
            if (underlayTile != null) e.RegisterHit(t, underlayTile);
            if (overlayTile != null) e.RegisterHit(t, overlayTile);
            e.RegisterHit(t, this);

            if (zone != null)
            {
                e.RegisterHit(t, zone);
                HashSet<IEventNode> printedEvents = new HashSet<IEventNode>();
                var zoneEvent = zone.Event;
                while (zoneEvent != null && !printedEvents.Contains(zoneEvent))
                {
                    e.RegisterHit(t, zoneEvent);
                    printedEvents.Add(zoneEvent);
                    zoneEvent = zoneEvent.NextEvent;
                } 
            }

            if (_lastHighlightIndex != highlightIndex)
            {
                HighlightIndexChanged?.Invoke(this, highlightIndex);
                _lastHighlightIndex = highlightIndex;
            }
        }
    }
}