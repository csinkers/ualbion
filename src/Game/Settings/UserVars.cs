using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Settings;

#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA1724 // Type names should not match namespaces
namespace UAlbion.Game.Settings;

public class UserVars
{
    UserVars() { }
    public static VarLibrary Library { get; } = new();
    public static UserVars Instance { get; } = new();

    public DebugVars Debug { get; } = new();
    public class DebugVars
    {
        public CustomVar<DebugFlags, int> DebugFlags { get; } = new(Library, "User.Debug.Flags", 0, x => (int)x, x => (DebugFlags)x, j => j.GetInt32());
    }

    public AudioVars Audio { get; } = new();
    public class AudioVars
    {
        public IntVar FxVolume { get; } = new(Library, "User.Audio.FxVolume", 48);
        public IntVar MusicVolume { get; } = new(Library, "User.Audio.MusicVolume", 32);

    }

    public GameplayVars Gameplay { get; } = new();
    public class GameplayVars
    {
        public StringVar Language { get; } = new(Library, "User.Gameplay.Language", Base.Language.English);
        public IntVar CombatDelay { get; } = new(Library, "User.Gameplay.Combat.MessageDelay", 3);
        public CustomVar<List<string>, string> ActiveMods { get; } = new(
            Library,
            "User.Gameplay.ActiveMods",
            ["Albion"],
            x => string.Join(",", x),
            x => (x ?? "").Split(',').ToList(),
            j => j.GetString());

    }

    public PathVars Path { get; } = new();
    public class PathVars
    {
        public StringVar Albion { get; } = new(Library, "User.Path.ALBION", "ALBION");
        public StringVar Saves { get; } = new(Library, "User.Path.SAVE", "$(ALBION)/SAVES");

    }
}
#pragma warning restore CA1724 // Type names should not match namespaces
#pragma warning restore CA1034 // Nested types should not be visible