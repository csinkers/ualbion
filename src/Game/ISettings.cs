using UAlbion.Core;
using UAlbion.Game.Settings;

namespace UAlbion.Game;

public interface ISettings
{
    IDebugSettings Debug { get; }
    IAudioSettings Audio { get; }
    IGameplaySettings Gameplay { get; }
    IEngineSettings Engine { get; }
    void Save();
}