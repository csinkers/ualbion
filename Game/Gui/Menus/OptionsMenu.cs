using System;
using System.Collections.Generic;
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
        int _windowSize3d;
        int _combatDetailLevel;
        int _combatDelay;

        public OptionsMenu() : base(null, DialogPositioning.Center)
        {
            var elements = new List<IUiElement>
            {
                new Spacing(156,2),
                new Label(SystemTextId.Options_MusicVolume.ToId()),
                new Slider(() => _musicVolume, x => _musicVolume = x, 0, 127),
                new Spacing(0,2),
                new Label(SystemTextId.Options_FXVolume.ToId()),
                new Slider(() => _fxVolume, x => _fxVolume = x, 0, 127),
                new Spacing(0,2),
                new Label(SystemTextId.Options_3DWindowSize.ToId()),
                new Slider(() => _windowSize3d, x => _windowSize3d = x, 0, 100),
                new Spacing(0,2),
                new Label(SystemTextId.Options_CombatDetailLevel.ToId()),
                new Slider(() => _combatDetailLevel, x => _combatDetailLevel = x, 1, 5),
                new Spacing(0,2),
                new Label(SystemTextId.Options_CombatTextDelay.ToId()),
                new Slider(() => _combatDelay, x => _combatDelay = x, 1, 50),
                new Spacing(0,2),
                new Button(SystemTextId.MsgBox_OK.ToId(), SaveAndClose),
                new Spacing(0,2),
            };
            var stack = new VerticalStack(elements);
            AttachChild(new DialogFrame(stack));
        }

        public override void Subscribed()
        {
            var settings = Resolve<ISettings>();
            _musicVolume = settings.Audio.MusicVolume;
            _fxVolume = settings.Audio.FxVolume;
            _windowSize3d = settings.Graphics.WindowSize3d;
            _combatDetailLevel = settings.Graphics.CombatDetailLevel;
            _combatDelay = settings.Gameplay.CombatDelay;
            base.Subscribed();
        }

        void SaveAndClose()
        {
            var settings = Resolve<ISettings>();
            if (_musicVolume != settings.Audio.MusicVolume) Raise(new SetMusicVolumeEvent(_musicVolume));
            if (_fxVolume != settings.Audio.FxVolume) Raise(new SetFxVolumeEvent(_fxVolume));
            if (_windowSize3d != settings.Graphics.WindowSize3d) Raise(new SetWindowSize3dEvent(_windowSize3d));
            if (_combatDetailLevel != settings.Graphics.CombatDetailLevel) Raise(new SetCombatDetailLevelEvent(_combatDetailLevel));
            if (_combatDelay != settings.Gameplay.CombatDelay) Raise(new SetCombatDelayEvent(_combatDelay));

            Closed?.Invoke(this, EventArgs.Empty);
            Detach();
        }
    }
}
