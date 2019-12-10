using UAlbion.Game.Settings;

namespace UAlbion.Game
{
    public interface ISettings
    {
        string BasePath { get; }
        IDebugSettings Debug { get; }
        IAudioSettings Audio { get; }
        IGraphicsSettings Graphics { get; }
        IGameplaySettings Gameplay { get; }
    }
}