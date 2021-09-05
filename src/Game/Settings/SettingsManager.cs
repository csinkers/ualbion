using System;
using System.Text.Json.Serialization;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;

namespace UAlbion.Game.Settings
{
    public class SettingsManager : Component, ISettings
    {
        readonly GeneralSettings _settings;

        [JsonIgnore] public IDebugSettings Debug => _settings;
        [JsonIgnore] public IAudioSettings Audio => _settings;
        [JsonIgnore] public IGameplaySettings Gameplay => _settings;
        [JsonIgnore] public IEngineSettings Engine => _settings;

        public SettingsManager(GeneralSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            On<SetLanguageEvent>(e =>
            {
                if (_settings.Language == e.Language)
                    return;
                _settings.Language = e.Language;
            });

            On<SetMusicVolumeEvent>(e => _settings.MusicVolume = e.Value);
            On<SetFxVolumeEvent>(e => _settings.FxVolume = e.Value);
            On<SetCombatDelayEvent>(e => _settings.CombatDelay = e.Value);
            On<DebugFlagEvent>(e =>
            {
                _settings.DebugFlags = (DebugFlags)CoreUtil.UpdateFlag((uint)_settings.DebugFlags, e.Operation, (uint)e.Flag);
                TraceAttachment = (_settings.DebugFlags & DebugFlags.TraceAttachment) != 0;
            });
            On<SpecialEvent>(e => _settings.Special1 = CoreUtil.UpdateValue(_settings.Special1, e.Operation, e.Argument));
            On<Special2Event>(e => _settings.Special2 = CoreUtil.UpdateValue(_settings.Special2, e.Operation, e.Argument));
            On<EngineFlagEvent>(e => _settings.Flags = (EngineFlags)CoreUtil.UpdateFlag((uint)_settings.Flags, e.Operation, (uint)e.Flag));
        }

        public void Save() => _settings.Save(Resolve<IGeneralConfig>(), Resolve<IFileSystem>(), Resolve<IJsonUtil>());

        protected override void Subscribed()
        {
            Exchange.Register<ISettings>(this);
            Exchange.Register<IDebugSettings>(_settings);
            Exchange.Register<IAudioSettings>(_settings);
            Exchange.Register<IGameplaySettings>(_settings);
            Exchange.Register<IEngineSettings>(_settings);
        }

        protected override void Unsubscribed() => Exchange.Unregister(this);
    }
}
