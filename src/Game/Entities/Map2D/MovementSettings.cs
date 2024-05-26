using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Entities.Map2D;

public class MovementSettings : IMovementSettings
{
    public MovementSettings(IDictionary<SpriteAnimation, int[]> frames) => Frames = frames;
    public IDictionary<SpriteAnimation, int[]> Frames { get; }
    public bool CanSit { get; set; } = true;
    public int TicksPerTile { get; set; } // Number of game ticks it takes to move across a map tile
    public int TicksPerFrame { get; set; } // Number of game ticks it takes to advance to the next animation frame
    public int MinTrailDistance { get; set; }
    public int MaxTrailDistance { get; set; }
    public int TileWidth => 16;
    public int TileHeight => 16;
    public float GetDepth(float y) => DepthUtil.GetAbsDepth(y);
    public int GetSpriteFrame(IMovementState state, Func<int, int, SitMode> getSitMode)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(getSitMode);
        var sitMode = getSitMode(state.X, state.Y);

        var anim = state.FacingDirection switch
        {
            Direction.West => SpriteAnimation.WalkW,
            Direction.East => SpriteAnimation.WalkE,
            Direction.North => SpriteAnimation.WalkN,
            Direction.South => SpriteAnimation.WalkS,
            _ => SpriteAnimation.Sleeping
        };

        anim = sitMode switch
        {
            SitMode.NorthLeft => SpriteAnimation.SitN,
            SitMode.NorthRight => SpriteAnimation.SitN,
            SitMode.East => SpriteAnimation.SitE,
            SitMode.SouthLeft => SpriteAnimation.SitS,
            SitMode.SouthRight => SpriteAnimation.SitS,
            SitMode.West => SpriteAnimation.SitW,
            SitMode.Sleep => SpriteAnimation.Sleeping,
            SitMode.Sleep2 => SpriteAnimation.Sleeping,
            _ => anim
        };

        var frames = Frames[anim];
        return frames[(state.MovementTick / TicksPerFrame) % frames.Length];
    }
}