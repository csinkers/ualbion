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

        protected Settings()
        {
            On<SetLanguageEvent>(e =>
            {
                if (Language == e.Language) return;
                Language = e.Language;
                Raise(e); // Re-raise to ensure any consumers who received it before Settings will get it again.
            });
            On<SetMusicVolumeEvent>(e => MusicVolume = e.Value);
            On<SetFxVolumeEvent>   (e => FxVolume    = e.Value);
            On<SetCombatDelayEvent>(e => CombatDelay = e.Value);
            On<DebugFlagEvent>     (e =>
            {
                DebugFlags = (DebugFlags) CoreUtil.UpdateFlag((uint) DebugFlags, e.Operation, (uint) e.Flag);
                Component.TraceAttachment = (DebugFlags & DebugFlags.TraceAttachment) != 0;
            });
            On<SpecialEvent>       (e => Special1 = CoreUtil.UpdateValue(Special1, e.Operation, e.Argument));
            On<Special2Event>      (e => Special2 = CoreUtil.UpdateValue(Special2, e.Operation, e.Argument));
            On<EngineFlagEvent>(e => Flags = (EngineFlags) CoreUtil.UpdateFlag((uint) Flags, e.Operation, (uint) e.Flag));
        }
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

        protected override void Subscribed()
        {
            Exchange.Register<ISettings>(this);
            Exchange.Register<IEngineSettings>(this);
            Exchange.Register<IDebugSettings>(this);
            Exchange.Register<IGameplaySettings>(this);
        }

        protected override void Unsubscribed()
        {
            Exchange.Unregister<ISettings>(this);
            Exchange.Unregister<IEngineSettings>(this);
            Exchange.Unregister<IDebugSettings>(this);
            Exchange.Unregister<IGameplaySettings>(this);
        }
    }
}
