using System.Collections.Generic;

namespace UAlbion.Formats.Assets;

public interface IMovementSettings
{
    IDictionary<SpriteAnimation, int[]> Frames { get; }
    int TicksPerTile { get; } // Number of game ticks it takes to move across a map tile
    int TicksPerFrame { get; } // Number of game ticks it takes to advance to the next animation frame
    int MinTrailDistance { get; } 
    int MaxTrailDistance { get; } // Max number of positions between each character in the party. Looks best if coprime to TicksPerPile and TicksPerFrame.
    int TileWidth { get; }
    int TileHeight { get; }
    float GetDepth(float y);
    int GetSpriteFrame(IMovementState state, bool isSeated);
}