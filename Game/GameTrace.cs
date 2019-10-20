using System.Diagnostics.Tracing;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game
{
    [EventSource(Name="UAlbion-GameTrace")]
    class GameTrace : EventSource
    {
        public static GameTrace Log { get; } = new GameTrace();

        public void AssetLoaded(AssetType type, int id, string name, GameLanguage language, string path)
        {
            WriteEvent(1, type, id, name, language, path);
        }
    }
}