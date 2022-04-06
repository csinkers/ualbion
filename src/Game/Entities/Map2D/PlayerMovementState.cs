using System;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities.Map2D;

public class PlayerMovementState : IMovementState
{
    public PlayerMovementState(MovementSettings settings) => Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    public MovementSettings Settings { get; }
    public bool NoClip { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }
    public ushort MoveToX { get; set; }
    public ushort MoveToY { get; set; }
    public float PixelX { get; set; }
    public float PixelY { get; set; }
    public int StartTick { get; set; }
    public int MovementTick { get; set; }
    public Direction FacingDirection { get; set; }
    public bool HasTarget { get; set; }
}