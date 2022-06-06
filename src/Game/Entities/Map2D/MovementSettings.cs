using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities.Map2D;

public class MovementSettings : IMovementSettings
{
    public MovementSettings(IDictionary<SpriteAnimation, int[]> frames) => Frames = frames;
    public IDictionary<SpriteAnimation, int[]> Frames { get; }
    public int TicksPerTile { get; set; } // Number of game ticks it takes to move across a map tile
    public int TicksPerFrame { get; set; } // Number of game ticks it takes to advance to the next animation frame
    public int MinTrailDistance { get; set; }
    public int MaxTrailDistance { get; set; }
    public int TileWidth => 16;
    public int TileHeight => 16;
    public float GetDepth(float y) => DepthUtil.GetAbsDepth(y);
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