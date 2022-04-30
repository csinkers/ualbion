using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Entities.Map2D;

public class MovementSettings : IMovementSettings
{
    readonly bool _isLarge;
    readonly Func<GameConfig.MovementT> _getConfig;

    public MovementSettings(bool isLarge, Func<GameConfig.MovementT> getConfig)
    {
        _isLarge = isLarge;
        _getConfig = getConfig ?? throw new ArgumentNullException(nameof(getConfig));
        Frames = isLarge ? LargeSpriteAnimations.Frames : SmallSpriteAnimations.Frames;
    }

    GameConfig.MovementT Movement => _getConfig();
    public IDictionary<SpriteAnimation, int[]> Frames { get; }
    public int TicksPerTile => Movement.TicksPerTile; // Number of game ticks it takes to move across a map tile
    public int TicksPerFrame => Movement.TicksPerFrame; // Number of game ticks it takes to advance to the next animation frame
    public int MinTrailDistance => _isLarge ? Movement.MinTrailDistanceLarge : Movement.MinTrailDistanceSmall;
    public int MaxTrailDistance => _isLarge ? Movement.MaxTrailDistanceLarge : Movement.MaxTrailDistanceSmall;
    public int TileWidth => 16;
    public int TileHeight => 16;
    public float GetDepth(float y) => DepthUtil.LayerToDepth(0, y);
    public int GetSpriteFrame(IMovementState state, bool isSeated)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        var anim = (state.FacingDirection, isSeated) switch
        {
            (Direction.West, false) => SpriteAnimation.WalkW,
            (Direction.East, false) => SpriteAnimation.WalkE,
            (Direction.North, false) => SpriteAnimation.WalkN,
            (Direction.South, false) => SpriteAnimation.WalkS,
            (Direction.West, true) => SpriteAnimation.SitW,
            (Direction.East, true) => SpriteAnimation.SitE,
            (Direction.North, true) => SpriteAnimation.SitN,
            (Direction.South, true) => SpriteAnimation.SitS,
            _ => SpriteAnimation.Sleeping
        };

        var frames = Frames[anim];
        return frames[(state.MovementTick / TicksPerFrame) % frames.Length];
    }
}