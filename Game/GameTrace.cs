using System.Diagnostics.Tracing;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Assets;
using UAlbion.Game.Entities.Map2D;

namespace UAlbion.Game
{
    [EventSource(Name="UAlbion-GameTrace")]
    class GameTrace : EventSource
    {
        public static GameTrace Log { get; } = new GameTrace();

        [NonEvent]
        public void AssetLoaded(AssetKey key, string name, string path)
        {
            AssetLoaded(key.Type, key.Id, name, key.Language, path);
        }

        [Event(1)]
        void AssetLoaded(AssetType type, int id, string name, GameLanguage language, string path)
        {
            WriteEvent(1, type, id, name, language, path);
        }

        public void Move(MovementDirection oldDir, MovementDirection desiredDir, MovementDirection facingDir, float fromX, float fromY, float toX, float toY, int frame)
        {
            WriteEvent(2, oldDir, desiredDir, facingDir, fromX, fromY, toX, toY, frame);
        }
    }
}
