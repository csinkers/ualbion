using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UAlbion.Api;
using UAlbion.Core;

namespace UAlbion.Game.Settings
{
    public class GeneralSettings : IDebugSettings, IAudioSettings, IGameplaySettings, IEngineSettings
    {
        string _configPath;

        // Debug
        public DebugFlags DebugFlags { get; set; }

        // Audio
        public int MusicVolume { get; set; } = 32;
        public int FxVolume { get; set; } = 48;

        // Graphics

        // Gameplay
        public string Language { get; set; } = Base.Language.English;
        public int CombatDelay { get; set; } = 3;
        public IList<string> ActiveMods { get; } = new List<string>();

        // Engine
        public float Special1 { get; set; }
        public float Special2 { get; set; }
        public EngineFlags Flags { get; set; } = 
            EngineFlags.VSync | 
            EngineFlags.UseCylindricalBillboards;

        public static GeneralSettings Load(string configPath, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            var settings = disk.FileExists(configPath) 
                ? JsonConvert.DeserializeObject<GeneralSettings>(disk.ReadAllText(configPath)) 
                : new GeneralSettings();

            settings._configPath = configPath;
            if (!settings.ActiveMods.Any())
                settings.ActiveMods.Add("Base");
            return settings;
        }

        public void Save(IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            var serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(this, serializerSettings);
            disk.WriteAllText(_configPath, json);
        }
    }
}
