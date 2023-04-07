using System;
using System.IO;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats;
using UAlbion.Formats.Ids;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Events;

namespace UAlbion.Game.Settings;

#pragma warning disable CA2227 // Collection properties should be read only
public class SettingsManager : Component, ISettings
{
    const int ConfigVersion = 1;
    static readonly IntVar Version = new("ConfigVersion", 0);
    const string VarSetName = "Settings";
    const string UserPath = "$(CONFIG)/settings.json";
    VarSet _set = new(VarSetName);

    public SettingsManager()
    {
        On<AssetUpdatedEvent>(e =>
        {
            if (e.Id == (AssetId)(SpecialId)Base.Special.GameConfig)
                Reload();
        });
        On<SetLanguageEvent>(e =>
        {
            if (Var(UserVars.Gameplay.Language) == e.Language)
                return;

            SetVar(UserVars.Gameplay.Language, e.Language);
        });

        On<SetMusicVolumeEvent>(e => SetVar(UserVars.Audio.MusicVolume, e.Value));
        On<SetFxVolumeEvent>(e => SetVar(UserVars.Audio.FxVolume, e.Value));
        On<SetCombatDelayEvent>(e => SetVar(UserVars.Gameplay.CombatDelay, e.Value));
        On<DebugFlagEvent>(e =>
        {
            var debugFlags = Var(UserVars.Debug.DebugFlags);
            debugFlags = (DebugFlags)CoreUtil.UpdateFlag((uint)debugFlags, e.Operation, (uint)e.Flag);
            TraceAttachment = (debugFlags & DebugFlags.TraceAttachment) != 0;
            SetVar(UserVars.Debug.DebugFlags, debugFlags);
        });
        On<SpecialEvent>(e =>
        {
            var value = Var(CoreVars.User.Special1);
            value = CoreUtil.UpdateValue(value, e.Operation, e.Argument);
            SetVar(CoreVars.User.Special1, value);
        });
        On<Special2Event>(e =>
        {
            var value = Var(CoreVars.User.Special2);
            value = CoreUtil.UpdateValue(value, e.Operation, e.Argument);
            SetVar(CoreVars.User.Special2, value);
        });
        On<EngineFlagEvent>(e =>
        {
            var value = Var(CoreVars.User.EngineFlags);
            value = (EngineFlags)CoreUtil.UpdateFlag((uint)value, e.Operation, (uint)e.Flag);
            SetVar(CoreVars.User.EngineFlags, value);
        });
    }

    void SetVar<T>(IVar<T> varInfo, T value)
    {
        if (varInfo == null) throw new ArgumentNullException(nameof(varInfo));
        varInfo.Write(this, value);
    }

    protected override void Subscribed() => Reload();
    protected override void Subscribing()
    {
        Exchange.Register(typeof(ISettings), this, false);
        Exchange.Register(typeof(IVarSet), this, false);
    }

    protected override void Unsubscribed()
    {
        Exchange.Unregister(typeof(IVarSet), this);
        Exchange.Unregister(typeof(ISettings), this);
    }

    // When the first load happens there will only be settings, no underlying config as the asset system hasn't been set up yet.
    // After the mods are loaded, Reload() is called manually to ensure that the values from config.json etc come through.
    public void Reload()
    {
        var disk = Resolve<IFileSystem>();
        var pathResolver = Resolve<IPathResolver>();
        var jsonUtil = Resolve<IJsonUtil>();
        var path = pathResolver.ResolvePath(UserPath);

        if (disk.FileExists(path))
        {
            _set = VarSetLoader.Load(VarSetName, path, disk, jsonUtil);

            // Note: If version doesn't match discard old config and go back to defaults.
            // This is just to clear out any bad entries from before the format was stabilised,
            // if future changes are made to the format we can implement an actual upgrade process.
            if (Version.Read(_set) != ConfigVersion)
            {
                Warn($"Settings file was not version {ConfigVersion} - discarding settings");
                _set = new VarSet(VarSetName);
            }
            else
                _set.ClearValue(Version.Key);
        }

        // The global mapping may be empty for unit tests that construct the asset system statically 
        if (AssetMapping.Global.IsEmpty)
            return;

        var assets = TryResolve<IAssetManager>();
        var baseConfig = assets?.LoadConfig();

        if (baseConfig == null) 
            return;

        _set.Apply(baseConfig);

        var registry = TryResolve<IVarRegistry>();
        if (registry != null)
        {
            foreach (var key in _set.Keys)
                if (!registry.IsVarRegistered(key))
                    Error($"Setting \"{key}\" has a value, but no Var has been registered with that key");
        }
        else
            Warn("No VarRegistry found, skipping var validation");
    }

    public void Save()
    {
        var pathResolver = Resolve<IPathResolver>();
        var disk = Resolve<IFileSystem>();
        var jsonUtil = Resolve<IJsonUtil>();

        var path = pathResolver.ResolvePath(UserPath);
        var dir = Path.GetDirectoryName(path);
        if (!disk.DirectoryExists(dir))
            disk.CreateDirectory(dir);

        Version.Write(this, ConfigVersion);
        VarSetLoader.Save(_set, path, disk, jsonUtil);
        _set.ClearValue(Version.Key);
    }

    public bool TryGetValue(string key, out object value) => _set.TryGetValue(key, out value);
    public void SetValue(string key, object value) => _set.SetValue(key, value);
    public void ClearValue(string key) => _set.ClearValue(key);
}
#pragma warning restore CA2227 // Collection properties should be read only