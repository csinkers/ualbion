using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Entities.Map2D;

public class HighlightTileAnnotation : Component
{
}

public class HighlightZoneAnnotation : Component
{
}

public class ShowPassabilityAnnotation : Component
{
}

public class MapAnnotationLayer : Component
{
    public MapAnnotationLayer(LogicalMap2D logicalMap, Vector2 tileSize)
    {
        _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));
        _tileSize = tileSize;
        _logicalMap.Dirty += (sender, args) => _dirty.Add((args.X, args.Y));
        // _drawLayer = DrawLayer.Diagnostic;
        On<PrepareFrameEvent>(e => Render());
    }

    readonly LogicalMap2D _logicalMap;
    readonly Vector2 _tileSize;
    // readonly DrawLayer _drawLayer;
    readonly ISet<(int, int)> _dirty = new HashSet<(int, int)>();

#if DEBUG
    DebugFlags _lastDebugFlags;
#endif
    // bool _allDirty = true;
    public int? HighlightIndex { get; set; }
    int? _highlightEvent;

    protected override void Unsubscribed()
    {
    }

    void BuildInstanceData(int i, int j)
    {
        var index = _logicalMap.Index(i, j);

        var position = new Vector3(new Vector2(i, j) * _tileSize, 1.0f);

        var underlay = _logicalMap.GetUnderlay(index);
        var overlay = _logicalMap.GetOverlay(index);
        //var zone = _logicalMap.GetZone
//#if DEBUG
//        bool showCollision = (_lastDebugFlags & DebugFlags.HighlightCollision) != 0;
//        if(showCollision)
//        {
//            var passability = _logicalMap.GetPassability(index);
//        }
//#endif
    }

    void Render()
    {
#if DEBUG
        var debug = ReadVar(V.User.Debug.DebugFlags);
        //if (_lastDebugFlags != debug)
        //    _allDirty = true;
        _lastDebugFlags = debug;
#endif

        if (HighlightIndex.HasValue)
        {
            var zone = _logicalMap.GetZone(HighlightIndex.Value);
            _highlightEvent = zone?.Node?.Id ?? -1;
            if (_highlightEvent == -1)
                _highlightEvent = null;
        }
        else _highlightEvent = null;

/*
            bool highlightTile = (_lastDebugFlags & DebugFlags.HighlightTile) != 0;
            int eventNum = zone?.Node?.Id ?? -1;
            bool highlightChain = (_lastDebugFlags & DebugFlags.HighlightEventChainZones) != 0 && _highlightEvent == eventNum;

            if (_allDirty)
            {
                for (int j = 0; j < _logicalMap.Height; j++)
                {
                    for (int i = 0; i < _logicalMap.Width; i++)
                    {
                        BuildInstanceData(i, j, tile);
                    }
                }

                _dirty.Clear();
                _allDirty = false;
            }
            else if (_dirty.Count > 0)
            {
                foreach (var (x,y) in _dirty)
                {
                    BuildInstanceData(x, y, tile);
                }
                _dirty.Clear();
            }
*/
    }
}