using UAlbion.Formats;

namespace UAlbion.Game
{
    public interface ISettings
    {
        GameLanguage Language { get; }
        int MusicVolume { get; }
        int FxVolume { get; }
        int WindowSize3d { get; }
        int CombatDetailLevel { get; }
        int CombatDelay { get; }
        string BasePath { get; }
    }
}