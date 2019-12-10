using System;
using System.Collections.Generic;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui
{
    public class OptionsMenu : Dialog
    {
        const string MusicVolumeKey = "Options.MusicVolume";
        const string FxVolumeKey = "Options.FxVolume";
        const string WindowSizeKey = "Options.WindowSize";
        const string CombatDetailKey = "Options.CombatDetail";
        const string CombatDelayKey = "Options.CombatDelay";
        const string DoneKey = "Options.Done";

        public event EventHandler Closed;

        int _musicVolume;
        int _fxVolume;
        int _windowSize3d;
        int _combatDetailLevel;
        int _combatDelay;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<OptionsMenu, ButtonPressEvent>((x, e) =>
            {
                switch(e.ButtonId)
                {
                    case DoneKey:
                        var settings = x.Resolve<ISettings>();
                        if(x._musicVolume != settings.Audio.MusicVolume)
                            x.Raise(new SetMusicVolumeEvent(x._musicVolume));
                        if(x._fxVolume != settings.Audio.FxVolume)
                            x.Raise(new SetFxVolumeEvent(x._fxVolume));
                        if(x._windowSize3d != settings.Graphics.WindowSize3d)
                            x.Raise(new SetWindowSize3dEvent(x._windowSize3d));
                        if(x._combatDetailLevel != settings.Graphics.CombatDetailLevel)
                            x.Raise(new SetCombatDetailLevelEvent(x._combatDetailLevel));
                        if(x._combatDelay != settings.Gameplay.CombatDelay)
                            x.Raise(new SetCombatDelayEvent(x._combatDelay));

                        x.Closed?.Invoke(x, EventArgs.Empty);
                        x.Detach();
                        break;
                }
            })
        );

        public OptionsMenu() : base(Handlers, DialogPositioning.Center)
        {
            StringId S(SystemTextId id) => new StringId(AssetType.SystemText, 0, (int)id);
            var elements = new List<IUiElement>
            {
                new Padding(156,2),
                new Label(S(SystemTextId.Options_MusicVolume)),
                new Slider(MusicVolumeKey, () => _musicVolume, x => _musicVolume = x, 0, 127),
                new Padding(0,2),
                new Label(S(SystemTextId.Options_FXVolume)),
                new Slider(FxVolumeKey, () => _fxVolume, x => _fxVolume = x, 0, 127),
                new Padding(0,2),
                new Label(S(SystemTextId.Options_3DWindowSize)),
                new Slider(WindowSizeKey, () => _windowSize3d, x => _windowSize3d = x, 0, 100),
                new Padding(0,2),
                new Label(S(SystemTextId.Options_CombatDetailLevel)),
                new Slider(CombatDetailKey, () => _combatDetailLevel, x => _combatDetailLevel = x, 1, 5),
                new Padding(0,2),
                new Label(S(SystemTextId.Options_CombatTextDelay)),
                new Slider(CombatDelayKey, () => _combatDelay, x => _combatDelay = x, 1, 50),
                new Padding(0,2),
                new Button(DoneKey, S(SystemTextId.MsgBox_OK)),
                new Padding(0,2),
            };
            var stack = new VerticalStack(elements);
            Children.Add(new DialogFrame(stack));
        }

        protected override void Subscribed()
        {
            var settings = Resolve<ISettings>();
            _musicVolume = settings.Audio.MusicVolume;
            _fxVolume = settings.Audio.FxVolume;
            _windowSize3d = settings.Graphics.WindowSize3d;
            _combatDetailLevel = settings.Graphics.CombatDetailLevel;
            _combatDelay = settings.Gameplay.CombatDelay;
            base.Subscribed();
        }
    }
}
