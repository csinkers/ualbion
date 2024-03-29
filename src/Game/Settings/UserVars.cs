﻿using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Settings;

#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA1724 // Type names should not match namespaces
namespace UAlbion.Game.Settings;

public static class UserVars
{
    public static class Debug
    {
        public static readonly CustomVar<DebugFlags, int> DebugFlags = new("User.Debug.Flags", 0, x => (int)x, x => (DebugFlags)x, j => j.GetInt32());
    }

    public static class Audio
    {
        public static readonly IntVar FxVolume    = new("User.Audio.FxVolume", 48);
        public static readonly IntVar MusicVolume = new("User.Audio.MusicVolume", 32);
    }

    public static class Gameplay
    {
        public static readonly StringVar Language = new("User.Gameplay.Language", Base.Language.English);
        public static readonly IntVar CombatDelay = new("User.Gameplay.Combat.MessageDelay", 3);
        public static readonly CustomVar<List<string>, string> ActiveMods = new(
            "User.Gameplay.ActiveMods",
            new List<string> { "Albion" },
            x => string.Join(",", x),
            x => (x ?? "").Split(',').ToList(),
            j => j.GetString());
    }

    public static class Path
    {
        public static readonly StringVar Albion = new("User.Path.ALBION", "ALBION");
        public static readonly StringVar Saves = new("User.Path.SAVE", "$(ALBION)/SAVES");
    }
}
#pragma warning restore CA1724 // Type names should not match namespaces
#pragma warning restore CA1034 // Nested types should not be visible