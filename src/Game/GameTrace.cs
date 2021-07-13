using System.Diagnostics.Tracing;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game
{
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
        {
            WriteEvent(1, type, id, name, language, path);
        }

        public void Move(Direction oldDir, Direction desiredDir, Direction facingDir, float fromX, float fromY, float toX, float toY, int frame)
        {
            WriteEvent(2, oldDir, desiredDir, facingDir, fromX, fromY, toX, toY, frame);
        }

        public void ClockStart(int stoppedFrames, float stoppedMs) => WriteEvent(3, stoppedFrames, stoppedMs);
        public void ClockStop() => WriteEvent(4);
        public void ClockUpdating(int cycles) => WriteEvent(5, cycles);
        public void ClockUpdateComplete() => WriteEvent(6);
    }
}
