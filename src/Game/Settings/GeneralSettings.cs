using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats;
using UAlbion.Game.Events;

namespace UAlbion.Game.Settings
{
    public class GeneralSettings : Component, ISettings, IDebugSettings, IAudioSettings, IGameplaySettings, IEngineSettings
    {
        string _configPath;

        protected GeneralSettings()
        {
            On<SetLanguageEvent>(e =>
            {
                if (Language == e.Language)
                    return;
                Language = e.Language;
            });
            On<SetMusicVolumeEvent>(e => MusicVolume = e.Value);
            On<SetFxVolumeEvent>(e => FxVolume = e.Value);
            On<SetCombatDelayEvent>(e => CombatDelay = e.Value);
            On<DebugFlagEvent>(e =>
            {
                DebugFlags = (DebugFlags)CoreUtil.UpdateFlag((uint)DebugFlags, e.Operation, (uint)e.Flag);
                TraceAttachment = (DebugFlags & DebugFlags.TraceAttachment) != 0;
            });
            On<SpecialEvent>(e => Special1 = CoreUtil.UpdateValue(Special1, e.Operation, e.Argument));
            On<Special2Event>(e => Special2 = CoreUtil.UpdateValue(Special2, e.Operation, e.Argument));
            On<EngineFlagEvent>(e => Flags = (EngineFlags)CoreUtil.UpdateFlag((uint)Flags, e.Operation, (uint)e.Flag));
        }

        [JsonIgnore] public IDebugSettings Debug => this;
        [JsonIgnore] public IAudioSettings Audio => this;
        [JsonIgnore] public IGameplaySettings Gameplay => this;
        [JsonIgnore] public IEngineSettings Engine => this;

        // Debug
        public DebugFlags DebugFlags { get; private set; }

        // Audio
        public int MusicVolume { get; private set; } = 32;
        public int FxVolume { get; private set; } = 48;

        // Graphics

        // Gameplay
        public GameLanguage Language { get; private set; } = GameLanguage.English;
        public int CombatDelay { get; private set; } = 3;
        public IList<string> Mods { get; } = new List<string>();

        // Engine
        public float Special1 { get; private set; }
        public float Special2 { get; private set; }
        public EngineFlags Flags { get; private set; } = 
            EngineFlags.VSync | 
            EngineFlags.UseCylindricalBillboards;

        public static GeneralSettings Load(string configPath)
        {
            var settings = File.Exists(configPath) 
                ? JsonConvert.DeserializeObject<GeneralSettings>(File.ReadAllText(configPath)) 
                : new GeneralSettings();

            settings._configPath = configPath;
            return settings;
        }

        public void Save()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(this, serializerSettings);
            File.WriteAllText(_configPath, json);
        }

        protected override void Subscribed()
        {
            Exchange.Register<ISettings>(this);
            Exchange.Register<IDebugSettings>(this);
            Exchange.Register<IAudioSettings>(this);
            Exchange.Register<IGameplaySettings>(this);
            Exchange.Register<IEngineSettings>(this);
        }

        protected override void Unsubscribed() => Exchange.Unregister(this);
    }
}
