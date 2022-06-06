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
            if (GetVar(UserVars.Gameplay.Language) == e.Language)
                return;
            SetVar(UserVars.Gameplay.Language, e.Language);
        });

        On<SetMusicVolumeEvent>(e => SetVar(UserVars.Audio.MusicVolume, e.Value));
        On<SetFxVolumeEvent>(e => SetVar(UserVars.Audio.FxVolume, e.Value));
        On<SetCombatDelayEvent>(e => SetVar(UserVars.Gameplay.CombatDelay, e.Value));
        On<DebugFlagEvent>(e =>
        {
            var debugFlags = GetVar(UserVars.Debug.DebugFlags);
            debugFlags = (DebugFlags)CoreUtil.UpdateFlag((uint)debugFlags, e.Operation, (uint)e.Flag);
            TraceAttachment = (debugFlags & DebugFlags.TraceAttachment) != 0;
            SetVar(UserVars.Debug.DebugFlags, debugFlags);
        });
        On<SpecialEvent>(e =>
        {
            var value = GetVar(CoreVars.User.Special1);
            value = CoreUtil.UpdateValue(value, e.Operation, e.Argument);
            SetVar(CoreVars.User.Special1, value);
        });
        On<Special2Event>(e =>
        {
            var value = GetVar(CoreVars.User.Special2);
            value = CoreUtil.UpdateValue(value, e.Operation, e.Argument);
            SetVar(CoreVars.User.Special2, value);
        });
        On<EngineFlagEvent>(e =>
        {
            var value = GetVar(CoreVars.User.EngineFlags);
            value = (EngineFlags)CoreUtil.UpdateFlag((uint)value, e.Operation, (uint)e.Flag);
            SetVar(CoreVars.User.EngineFlags, value);
        });
    }

    void SetVar<T>(IVar<T> varInfo, T value)
    {
        if (varInfo == null) throw new ArgumentNullException(nameof(varInfo));
        varInfo.Write(_set, value);
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
        if (AssetMapping.Global.IsEmpty)
            return;

        var disk = Resolve<IFileSystem>();
        var pathResolver = Resolve<IPathResolver>();
        var jsonUtil = Resolve<IJsonUtil>();
        var assets = TryResolve<IAssetManager>();

        var path = pathResolver.ResolvePath(UserPath);
        var baseConfig = assets?.LoadConfig();

        if (disk.FileExists(path))
            _set = VarSetLoader.Load(VarSetName, path, disk, jsonUtil);

        if (baseConfig != null)
            _set.Apply(baseConfig);
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

        var json = jsonUtil.Serialize(this);
        disk.WriteAllText(path, json);
    }

    public bool TryGetValue(string key, out object value) => _set.TryGetValue(key, out value);
    public void SetValue(string key, object value) => _set.SetValue(key, value);
    public void ClearValue(string key) => _set.ClearValue(key);
}
#pragma warning restore CA2227 // Collection properties should be read only