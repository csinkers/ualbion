using System.Numerics;

namespace UAlbion.Game.State
{
    public interface IStateManager
    {
        IGameState State { get; }
        int FrameCount { get; }
        Vector3 TileSize { get; }
    }
}