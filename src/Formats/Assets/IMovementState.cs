namespace UAlbion.Formats.Assets;

public interface IMovementState
{
    bool NoClip { get; }
    ushort X { get; set; }
    ushort Y { get; set; }
    ushort MoveToX { get; set; }
    ushort MoveToY { get; set; }
    float PixelX { get; set; }
    float PixelY { get; set; }
    int StartTick { get; set; }
    int MovementTick { get; set; }
    Direction FacingDirection { get; set; }
    bool HasTarget { get; set; }
}