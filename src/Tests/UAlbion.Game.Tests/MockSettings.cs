using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Tests
{
    public class MockSettings : Component, ISettings, IDebugSettings, IAudioSettings, IGameplaySettings, IEngineSettings
    {
        public void Save() { }
        public string BasePath { get; set; }
        public IDebugSettings Debug => this;
        public IAudioSettings Audio => this;
        public IGameplaySettings Gameplay => this;
        public IEngineSettings Engine => this;
        public DebugFlags DebugFlags { get; set; }
        public int MusicVolume { get; set; }
        public int FxVolume { get; set; }
        public GameLanguage Language { get; set; }
        public int CombatDelay { get; set; }
        public IList<string> ActiveMods { get; set; }
        public float Special1 { get; set; }
        public float Special2 { get; set; }
        public EngineFlags Flags { get; set; }

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