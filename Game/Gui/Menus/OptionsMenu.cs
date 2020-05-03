using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Menus
{
    public class OptionsMenu : Dialog
    {
        public event EventHandler Closed;

        int _musicVolume;
        int _fxVolume;
        int _combatDelay;

        bool HasLanguageFiles(GameLanguage language)
        {
            var config = Resolve<IAssetManager>().LoadGeneralConfig();
            var path = Path.Combine(config.BasePath, config.XldPath, language.ToString().ToUpper());
            return Directory.Exists(path);
        }

        public OptionsMenu() : base(DialogPositioning.Center) { }

        protected override void Subscribed()
        {
            var languageButtons = new List<IUiElement>();
            void SetLanguage(GameLanguage language) => Raise(new SetLanguageEvent(language));

            void AddLang(string label, GameLanguage language)
            {
                if (HasLanguageFiles(language))
                    languageButtons.Add(new Button(label, () => SetLanguage(language)));
            }

            AddLang("EN", GameLanguage.English);
            AddLang("DE", GameLanguage.German);
            AddLang("FR", GameLanguage.French);

            var elements = new List<IUiElement>
            {
                new Spacing(156,2),
                new Label(UAlbionStringId.LanguageLabel.ToId()),
                new HorizontalStack(languageButtons),
                new Spacing(0,2),
                new Label(SystemTextId.Options_MusicVolume.ToId()),
                new Slider(() => _musicVolume, x => _musicVolume = x, 0, 127),
                new Spacing(0,2),
                new Label(SystemTextId.Options_FXVolume.ToId()),
                new Slider(() => _fxVolume, x => _fxVolume = x, 0, 127),
                new Spacing(0,2),
                new Label(SystemTextId.Options_CombatTextDelay.ToId()),
                new Slider(() => _combatDelay, x => _combatDelay = x, 1, 50),
                new Spacing(0,2),
                new Button(SystemTextId.MsgBox_OK.ToId(), SaveAndClose),
                new Spacing(0,2),
            };
            var stack = new VerticalStack(elements);
            AttachChild(new DialogFrame(stack));

            var settings = Resolve<ISettings>();
            _musicVolume = settings.Audio.MusicVolume;
            _fxVolume = settings.Audio.FxVolume;
            _combatDelay = settings.Gameplay.CombatDelay;
        }

        void SaveAndClose()
        {
            var settings = Resolve<ISettings>();
            if (_musicVolume != settings.Audio.MusicVolume) Raise(new SetMusicVolumeEvent(_musicVolume));
            if (_fxVolume != settings.Audio.FxVolume) Raise(new SetFxVolumeEvent(_fxVolume));
            if (_combatDelay != settings.Gameplay.CombatDelay) Raise(new SetCombatDelayEvent(_combatDelay));
            settings.Save();

            Closed?.Invoke(this, EventArgs.Empty);
            Detach();
        }
    }
}
