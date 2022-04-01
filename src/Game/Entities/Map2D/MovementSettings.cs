using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Entities.Map2D;

public class MovementSettings : IMovementSettings
{
    readonly bool _isLarge;
    readonly IGameConfigProvider _config;
    readonly Func<float, float> _getDepth;

    public MovementSettings(bool isLarge, IGameConfigProvider config)
    {
        _isLarge = isLarge;
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _getDepth = isLarge ? DepthUtil.IndoorCharacterDepth : DepthUtil.OutdoorCharacterDepth;
        Frames = isLarge ? LargeSpriteAnimations.Frames : SmallSpriteAnimations.Frames;
    }

    GameConfig.MovementT Movement => _config.Game.Movement;

    public IDictionary<SpriteAnimation, int[]> Frames { get; }
    public int TicksPerTile => Movement.TicksPerTile; // Number of game ticks it takes to move across a map tile
    public int TicksPerFrame => Movement.TicksPerFrame; // Number of game ticks it takes to advance to the next animation frame
    public int MinTrailDistance => _isLarge ? Movement.MinTrailDistanceLarge : Movement.MinTrailDistanceSmall;
    public int MaxTrailDistance => _isLarge ? Movement.MaxTrailDistanceLarge : Movement.MaxTrailDistanceSmall;
    public int TileWidth => 16;
    public int TileHeight => 16;
    public float GetDepth(float y) => _getDepth(y);
}