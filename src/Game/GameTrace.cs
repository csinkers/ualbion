using System.Diagnostics.Tracing;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game;

[EventSource(Name="UAlbion-GameTrace")]
class GameTrace : EventSource
{
    public static GameTrace Log { get; } = new();

    [NonEvent]
    public void AssetLoaded(AssetId key, string language, string path)
    {
        AssetLoaded(key.Type, key.Id, key.ToString(), language, path);
    }

    [Event(1)]
    void AssetLoaded(AssetType type, int id, string name, string language, string path)
        => WriteEvent(1, type, id, name, language, path);

    public void ClockStart(int stoppedFrames, float stoppedMs) => WriteEvent(2, stoppedFrames, stoppedMs);
    public void ClockStop() => WriteEvent(3);
    public void ClockUpdating(int cycles) => WriteEvent(4, cycles);
    public void ClockUpdateComplete() => WriteEvent(5);
    public void SetNpcMoveTarget(int npc, int x, int y) => WriteEvent(6, npc, x, y);
    public void TeleportNpc(int npc, int x, int y) => WriteEvent(7, npc, x, y);
    public void MoveStart(string id, ushort x, ushort y, float pixelX, float pixelY) => WriteEvent(8, id, x, y, pixelX, pixelY);
    public void MoveDir(Direction oldDir, Direction desiredDir, Direction facingDir) => WriteEvent(9, oldDir, desiredDir, facingDir);
    public void MovePos(int frame, float fromX, float fromY, float toX, float toY) => WriteEvent(10, frame, fromX, fromY, toX, toY);
    public void MoveStop(string id, bool moved) => WriteEvent(11, id, moved);
}
