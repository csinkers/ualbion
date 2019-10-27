using System.Numerics;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.State
{
    public interface IStateManager
    {
        IGameState State { get; }
        int FrameCount { get; }
        PaletteId PaletteId { get; }
        Vector3 CameraTilePosition { get; }
        Vector2 CameraDirection { get; }
        Vector3 CameraPosition { get; }
        Vector3 TileSize { get; }
        float CameraMagnification { get; }
    }
}