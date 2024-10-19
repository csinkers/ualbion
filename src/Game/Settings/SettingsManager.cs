using System;
using System.IO;
using System.Linq;
using System.Text.Json;
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

[Event("save", "Save the current settings to disk")]
public record SaveSettingsEvent : EventRecord;

public class SettingsManager : GameComponent, ISettings
{
    const int ConfigVersion = 1;
    const string VarSetName = "Settings";
    const string UserPath = "$(CONFIG)/settings.json";
    VarSet _set = new(VarSetName);
    bool _dirty;

    public SettingsManager()
    {
        On<AssetUpdatedEvent>(e =>
        {
            if (e.Id == (AssetId)(SpecialId)Base.Special.GameConfig)
                Reload();
        });
        On<SetLanguageEvent>(e =>
        {
            if (ReadVar(V.User.Gameplay.Language) == e.Language)
                return;

            SetVar(V.User.Gameplay.Language, e.Language);
        });

        On<SaveSettingsEvent>(_ => Save());
        On<SetMusicVolumeEvent>(e => SetVar(V.User.Audio.MusicVolume, e.Value));
        On<SetFxVolumeEvent>(e => SetVar(V.User.Audio.FxVolume, e.Value));
        On<SetCombatDelayEvent>(e => SetVar(V.User.Gameplay.CombatDelay, e.Value));
        On<DebugFlagEvent>(e =>
        {
            var debugFlags = ReadVar(V.User.Debug.DebugFlags);
            debugFlags = (DebugFlags)CoreUtil.UpdateFlag((uint)debugFlags, e.Operation, (uint)e.Flag);
            TraceAttachment = (debugFlags & DebugFlags.TraceAttachment) != 0;
            SetVar(V.User.Debug.DebugFlags, debugFlags);
        });
        On<SpecialEvent>(e =>
        {
            var value = ReadVar(V.Core.User.Special1);
            value = CoreUtil.UpdateValue(value, e.Operation, e.Argument);
            SetVar(V.Core.User.Special1, value);
        });
        On<Special2Event>(e =>
        {
            var value = ReadVar(V.Core.User.Special2);
            value = CoreUtil.UpdateValue(value, e.Operation, e.Argument);
            SetVar(V.Core.User.Special2, value);
        });
        On<EngineFlagEvent>(e =>
        {
            var value = ReadVar(V.Core.User.EngineFlags);
            value = (EngineFlags)CoreUtil.UpdateFlag((uint)value, e.Operation, (uint)e.Flag);
            SetVar(V.Core.User.EngineFlags, value);
        });

        On<GetVarEvent>(e =>
        {
            var name = e.Name ?? "";
            var registry = TryResolve<IVarRegistry>();
            foreach (var v in registry.Vars)
            {
                if (!v.Key.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (!_set.TryGetValue(v.Key, out var value))
                    value = v.DefaultValueUntyped;

                Info($"{v.Key} {value} [default: {v.DefaultValueUntyped}]");
            }
        });

        On<SetVarEvent>(e =>
        {
            var target = GetSingleVar(e.Name ?? "");
            if (target == null)
                return;

            try
            {
                target.WriteFromString(this, e.Value);
            }
            catch (JsonException ex) { Error($"\"{e.Value}\" could not be converted to a valid {target.ValueType}: {ex.Message}"); }
            catch (FormatException ex) { Error($"\"{e.Value}\" could not be converted to a valid {target.ValueType}: {ex.Message}"); }
        });

        On<ResetVarEvent>(e =>
        {
            var target = GetSingleVar(e.Name ?? "");
            if (target == null)
                return;

            ClearValue(target.Key);
        });
    }

    IVar GetSingleVar(string name)
    {
        var registry = TryResolve<IVarRegistry>();
        var candidates = registry.Vars.Where(v => v.Key.Contains(name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        if (candidates.Count == 0)
        {
            Error($"No var found matching \"{name}\"");
            return null;
        }

        if (candidates.Count > 1)
        {
            Error($"Multiple vars found matching \"{name}\": {string.Join(", ", candidates.Select(x => x.Key))}");
            return null;
        }

        return candidates[0];
    }

    void SetVar<T>(IVar<T> varInfo, T value)
    {
        ArgumentNullException.ThrowIfNull(varInfo);
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
            int version = V.Game.Version.Read(_set);
            if (version != ConfigVersion)
            {
                Warn($"Settings file was not version {ConfigVersion} - discarding settings");
                _set = new VarSet(VarSetName);
            }
            else
                _set.ClearValue(V.Game.Version.Key);
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
        if (!_dirty)
            return;

        var pathResolver = Resolve<IPathResolver>();
        var disk = Resolve<IFileSystem>();
        var jsonUtil = Resolve<IJsonUtil>();

        var path = pathResolver.ResolvePath(UserPath);
        var dir = Path.GetDirectoryName(path);
        if (!disk.DirectoryExists(dir))
            disk.CreateDirectory(dir);

        V.Game.Version.Write(this, ConfigVersion);
        VarSetLoader.Save(_set, path, disk, jsonUtil);
        _set.ClearValue(V.Game.Version.Key);
        _dirty = false;
    }

    public bool TryGetValue(string key, out object value) => _set.TryGetValue(key, out value);
    public void SetValue(string key, object value)
    {
        _set.SetValue(key, value);
        _dirty = true;
    }

    public void ClearValue(string key)
    {
        _set.ClearValue(key);
        _dirty = true;
    }
}
