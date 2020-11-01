using System.Collections.Generic;
using UAlbion.Formats;

namespace UAlbion.Game.Settings
{
    public interface IGameplaySettings
    {
        GameLanguage Language { get; }
        int CombatDelay { get; }
        IList<string> Mods { get; }
    }
}
