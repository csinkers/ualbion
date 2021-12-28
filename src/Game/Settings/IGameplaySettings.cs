using System.Collections.Generic;

namespace UAlbion.Game.Settings;

public interface IGameplaySettings
{
    string Language { get; }
    int CombatDelay { get; }
    IList<string> ActiveMods { get; }
}