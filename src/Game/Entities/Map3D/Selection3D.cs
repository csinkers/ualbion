using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;

namespace UAlbion.Game.Entities.Map3D;

public class Selection3D : Component
{
    public Selection3D()
    {
        OnAsync<WorldCoordinateSelectEvent, Selection>(OnSelect);
    }

    bool OnSelect(WorldCoordinateSelectEvent e, Action<Selection> continuation)
    {
        var scene = TryResolve<ISceneGraph>();
        if (scene == null)
            return false;

        var hits = new List<Selection>(); // TODO: Get rid of the extra allocation and copying
        scene.RayIntersect(e.Origin, e.Direction, hits);
        foreach (var hit in hits)
            continuation(hit);
        return true;

        // Find floor / ceiling hit (furthest point)
        // Iterate all tiles on a straight-line path between origin and floor hit
        // For each tile, yield if filled and if empty iterate contents performing hit checks.
/*
            float denominator = Vector3.Dot(Normal, e.Direction);
            if (Math.Abs(denominator) < 0.00001f)
                return;

            float t = Vector3.Dot(-e.Origin, Normal) / denominator;
            if (t < 0)
                return;

            Vector3 intersectionPoint = e.Origin + t * e.Direction;
            int x = (int)(intersectionPoint.X / _renderable.TileSize.X);
            int y = (int)(intersectionPoint.Y / _renderable.TileSize.Y);

            int highlightIndex = y * _map.Width + x;
            var underlayTile = _map.GetUnderlay(x, y);
            var overlayTile = _map.GetOverlay(x, y);

            e.RegisterHit(t, new MapTileHit(
                new Vector2(x, y),
                intersectionPoint,
                _renderable.GetWeakUnderlayReference(x, y),
                _renderable.GetWeakOverlayReference(x, y)));

            if (underlayTile != null) e.RegisterHit(t, underlayTile);
            if (overlayTile != null) e.RegisterHit(t, overlayTile);
            e.RegisterHit(t, this);

            var zone = _map.GetZone(x, y);
            if (zone != null)
                e.RegisterHit(t, zone);

            var chain = zone?.Chain;
            if (chain != null)
            {
                foreach (var zoneEvent in chain.Events)
                    e.RegisterHit(t, zoneEvent);
            }

            if (_lastHighlightIndex != highlightIndex)
            {
                HighlightIndexChanged?.Invoke(this, highlightIndex);
                _lastHighlightIndex = highlightIndex;
            }
            */
    }
}