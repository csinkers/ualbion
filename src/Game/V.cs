using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Settings;

namespace UAlbion.Game;

internal static class V
{
    public static GameVars Game => GameVars.Instance;
    public static CoreVars Core => CoreVars.Instance;
    public static UserVars User => UserVars.Instance;
}