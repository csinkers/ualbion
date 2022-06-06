/*using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities.Map2D;
using UAlbion.Game.Settings;
using UAlbion.Game.State;

namespace UAlbion.Game.Veldrid.Visual;

public class MapLayer<TInstance> : Component, IMapLayer
    where TInstance : unmanaged
{
    readonly LogicalMap2D _logicalMap;
    readonly IMapLayerInfoBuilder<TInstance> _builder;
    readonly IMapLayerBehavior<TInstance> _behavior;
    readonly ISet<(int, int)> _dirty = new HashSet<(int, int)>();
    readonly ISet<(int, int)> _animated = new HashSet<(int, int)>();

#if DEBUG
    DebugFlags _lastDebugFlags;
#endif
    SpriteLease<TInstance> _lease;
    bool _allDirty = true;

    public int? HighlightIndex { get; set; }
    public int FrameNumber { get; set; }

    int? _highlightEvent;

    public MapLayer(
        LogicalMap2D logicalMap,
        IMapLayerInfoBuilder<TInstance> builder,
        IMapLayerBehavior<TInstance> behavior)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _behavior = behavior ?? throw new ArgumentNullException(nameof(behavior));

        _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));
        _logicalMap.Dirty += (_, args) =>
        {
            if (!_behavior.IsChangeApplicable(args.Type))
                return;

            var index = _logicalMap.Index(args.X, args.Y);

            if (_behavior.IsAnimated(index))
                _animated.Add((args.X, args.Y));
            else
                _animated.Remove((args.X, args.Y));

            _dirty.Add((args.X, args.Y));
        };


        int index = 0;
        for (int j = 0; j < _logicalMap.Height; j++)
        {
            for (int i = 0; i < _logicalMap.Width; i++)
            {
                if (_behavior.IsAnimated(index))
                    _animated.Add((i, j));
                index++;
            }
        }

        On<RenderEvent>(_ => Render());
    }

    public void SetTile(int index, int value)
    {
        // TODO
    }

    public object GetSpriteData(int x, int y) 
        => _lease?.GetInstance(_logicalMap.Index(x, y));

    protected override void Unsubscribed()
    {
        _lease?.Dispose();
        _lease = null;
    }

    void Render()
    {
        var frameCount =  (Resolve<IGameState>()?.TickCount ?? 0) / GetVar(GameVars.Time.FastTicksPerMapTileFrame);
#if DEBUG
        var debug = GetVar(UserVars.Debug.DebugFlags);
        if (_lastDebugFlags != debug)
            _allDirty = true;
        _lastDebugFlags = debug;

        if (frameCount != FrameNumber && (
                (debug & DebugFlags.HighlightChain) != 0
                || (debug & DebugFlags.HighlightTile) != 0))
            _allDirty = true;
#endif

        var sm = Resolve<ISpriteManager<TInstance>>();
        if (_lease == null)
        {
            var key = _behavior.GetSpriteKey();
            _lease = sm.Borrow(key, _logicalMap.Width * _logicalMap.Height, this);
            _allDirty = true;
        }

        if (_lease == null)
            return;

        if (HighlightIndex.HasValue)
        {
            var zone = _logicalMap.GetZone(HighlightIndex.Value);
            _highlightEvent = zone?.Node?.Id ?? -1;
            if (_highlightEvent == -1)
                _highlightEvent = null;
        }
        else _highlightEvent = null;

        if (!_allDirty && frameCount != FrameNumber)
            _dirty.UnionWith(_animated);

        FrameNumber = frameCount;
        if (_allDirty)
        {
            _lease.Access(static (instances, s) =>
            {
                int index = 0;
                for (int j = 0; j < s._logicalMap.Height; j++)
                {
                    for (int i = 0; i < s._logicalMap.Width; i++)
                    {
                        var position = new Vector3(new Vector2(i, j) * s._builder.TileSize, DepthUtil.GetAbsDepth(j));
                        instances[index] = s._behavior.BuildInstanceData(index, s.FrameNumber, position);
                        index++;
                    }
                }

                s._dirty.Clear();
                s._allDirty = false;
            }, this);
        }
        else if (_dirty.Count > 0)
        {
            _lease.Access(static (instances, s) =>
            {
                foreach (var (i, j) in s._dirty)
                {
                    var position = new Vector3(new Vector2(i, j) * s._builder.TileSize, DepthUtil.GetAbsDepth(j));
                    int index = s._logicalMap.Index(i, j);
                    instances[index] = s._behavior.BuildInstanceData(index, s.FrameNumber, position);
                }

                s._dirty.Clear();
            }, this);
        }
    }
} */