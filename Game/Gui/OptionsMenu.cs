using System;
using System.Collections.Generic;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui
{
    public class OptionsMenu : UiElement
    {
        const string MusicVolumeKey = "Options.MusicVolume";
        const string FxVolumeKey = "Options.FxVolume";
        const string WindowSizeKey = "Options.WindowSize";
        const string CombatDetailKey = "Options.CombatDetail";
        const string CombatDelayKey = "Options.CombatDelay";
        const string DoneKey = "Options.Done";

        public int MusicVolume { get; set; }
        public int FxVolume { get; set; }
        public int WindowSize3d { get; set; }
        public int CombatDetailLevel { get; set; }
        public int CombatDelay { get; set; }
        public event EventHandler Closed;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<OptionsMenu, ButtonPressEvent>((x, e) =>
            {
                switch(e.ButtonId)
                {
                    case DoneKey:
                        var settings = x.Exchange.Resolve<ISettings>();
                        if(x.MusicVolume != settings.MusicVolume)
                            x.Raise(new SetMusicVolumeEvent(x.MusicVolume));
                        if(x.FxVolume != settings.FxVolume)
                            x.Raise(new SetFxVolumeEvent(x.FxVolume));
                        if(x.WindowSize3d != settings.WindowSize3d)
                            x.Raise(new SetWindowSize3dEvent(x.WindowSize3d));
                        if(x.CombatDetailLevel != settings.CombatDetailLevel)
                            x.Raise(new SetCombatDetailLevelEvent(x.CombatDetailLevel));
                        if(x.CombatDelay != settings.CombatDelay)
                            x.Raise(new SetCombatDelayEvent(x.CombatDelay));

                        x.Closed?.Invoke(x, EventArgs.Empty);
                        x.Detach();
                        break;
                }
            })
        );

        public OptionsMenu() : base(Handlers)
        {
            StringId S(SystemTextId id) => new StringId(AssetType.SystemText, 0, (int)id);
            var elements = new List<IUiElement>
            {
                new Padding(0,2),
                new Label(S(SystemTextId.Options_MusicVolume)),
                new Slider(MusicVolumeKey, () => MusicVolume, x => MusicVolume = x, 0, 127),
                new Padding(0,2),
                new Label(S(SystemTextId.Options_FXVolume)),
                new Slider(FxVolumeKey, () => FxVolume, x => FxVolume = x, 0, 127),
                new Padding(0,2),
                new Label(S(SystemTextId.Options_3DWindowSize)),
                new Slider(WindowSizeKey, () => WindowSize3d, x => WindowSize3d = x, 0, 100),
                new Padding(0,2),
                new Label(S(SystemTextId.Options_CombatDetailLevel)),
                new Slider(CombatDetailKey, () => CombatDetailLevel, x => CombatDetailLevel = x, 1, 5),
                new Padding(0,2),
                new Label(S(SystemTextId.Options_CombatTextDelay)),
                new Slider(CombatDelayKey, () => CombatDelay, x => CombatDelay = x, 1, 50),
                new Padding(0,2),
                new Button(DoneKey, S(SystemTextId.MsgBox_OK)),
                new Padding(0,2),
            };
            var stack = new VerticalStack(elements);
            Children.Add(new Frame(stack));
        }

        protected override void Subscribed()
        {
            var layout = Exchange.Resolve<ILayoutManager>();
            layout.Add(this, DialogPositioning.Center);

            var settings = Exchange.Resolve<ISettings>();
            MusicVolume = settings.MusicVolume;
            FxVolume = settings.FxVolume;
            WindowSize3d = settings.WindowSize3d;
            CombatDetailLevel = settings.CombatDetailLevel;
            CombatDelay = settings.CombatDelay;
        }
    }
}
