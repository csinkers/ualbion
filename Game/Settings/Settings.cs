using System.IO;
using Newtonsoft.Json;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats;
using UAlbion.Game.Events;

namespace UAlbion.Game.Settings
{
    public class Settings : Component, ISettings, IDebugSettings, IAudioSettings, IGraphicsSettings, IGameplaySettings, IEngineSettings
    {
        const string Filename = "settings.json";
        static readonly HandlerSet Handlers = new HandlerSet(
            H<Settings, SetLanguageEvent>((x, e) =>
            {
                if (x.Language == e.Language) return;
                x.Language = e.Language;
                x.Raise(e); // Re-raise to ensure any consumers who received it before Settings will get it again.
            }),
            H<Settings, SetMusicVolumeEvent> ((x, e) => x.MusicVolume = e.Value),
            H<Settings, SetFxVolumeEvent>    ((x, e) => x.FxVolume    = e.Value),
            H<Settings, SetCombatDelayEvent> ((x, e) => x.CombatDelay = e.Value),
            H<Settings, DebugFlagEvent>      ((x, e) => x.DebugFlags = (DebugFlags)CoreUtil.UpdateFlag((uint)x.DebugFlags, e.Operation, (uint)e.Flag)),
            H<Settings, SpecialEvent>        ((x, e) => x.Special1 = CoreUtil.UpdateValue(x.Special1, e.Operation, e.Argument)),
            H<Settings, Special2Event>       ((x, e) => x.Special2 = CoreUtil.UpdateValue(x.Special2, e.Operation, e.Argument)),
            H<Settings, EngineFlagEvent>     ((x, e) => x.Flags = (EngineFlags)CoreUtil.UpdateFlag((uint)x.Flags, e.Operation, (uint)e.Flag))
        );

        protected Settings() : base(Handlers) { }
        [JsonIgnore] public string BasePath { get; set; }

        IDebugSettings ISettings.Debug => this;
        IAudioSettings ISettings.Audio => this;
        IGraphicsSettings ISettings.Graphics => this;
        IGameplaySettings ISettings.Gameplay => this;
        IEngineSettings ISettings.Engine => this;

        // Debug
        public DebugFlags DebugFlags { get; private set; }

        // Audio
        public int MusicVolume { get; private set; } = 32;
        public int FxVolume { get; private set; } = 48;

        // Graphics

        // Gameplay
        public GameLanguage Language { get; private set; } = GameLanguage.English;
        public int CombatDelay { get; private set; } = 3;

        // Engine
        public float Special1 { get; private set; }
        public float Special2 { get; private set; }
        public EngineFlags Flags { get; private set; } = 
            EngineFlags.VSync | 
            EngineFlags.UseCylindricalBillboards;

        public static Settings Load(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", Filename);
            if (!File.Exists(configPath))
                return new Settings { BasePath = basePath };

            var configText = File.ReadAllText(configPath);
            var settings = JsonConvert.DeserializeObject<Settings>(configText);
            settings.BasePath = basePath;
            return settings;
        }

        public void Save()
        {
            var configPath = Path.Combine(BasePath, "data", Filename);
            var serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(this, serializerSettings);
            File.WriteAllText(configPath, json);
        }
    }
}
