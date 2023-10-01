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
    public void MovePos(float fromX, float fromY, float toX, float toY, int frame) => WriteEvent(10, fromX, fromY, toX, toY, frame);
    public void MoveStop(string id, bool moved) => WriteEvent(11, id, moved);
    public void FastTick(int ticks) => WriteEvent(12, ticks);
    public void MinuteElapsed(int time) => WriteEvent(13, time);
    public void HourElapsed(int time) => WriteEvent(14, time);
    public void DayElapsed(int time) => WriteEvent(15, time);
    public void SlowTick(int ticks) => WriteEvent(16, ticks);
    public void IdleTick(int ticks) => WriteEvent(17, ticks);
    public void CombatClockUpdating(int ticks) => WriteEvent(18, ticks);
    public void CombatTick(int tick) => WriteEvent(19, tick);
    public void CombatClockStart(int stoppedFrames, float stoppedMs) => WriteEvent(20, stoppedFrames, stoppedMs);
    public void CombatClockStop() => WriteEvent(21);
}
