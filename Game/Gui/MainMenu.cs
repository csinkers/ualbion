using System.Collections.Generic;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui
{
    public class MainMenu : UiElement
    {
        const string ContinueKey  = "MainMenu.ContinueGame";
        const string NewGameKey   = "MainMenu.NewGame";
        const string LoadGameKey  = "MainMenu.LoadGame";
        const string SaveGameKey  = "MainMenu.SaveGame";
        const string OptionsKey   = "MainMenu.Options";
        const string ViewIntroKey = "MainMenu.ViewIntro";
        const string CreditsKey   = "MainMenu.Credits";
        const string QuitGameKey  = "MainMenu.QuitGame";

        static readonly HandlerSet Handlers = new HandlerSet(
            H<MainMenu, ButtonPressEvent>((x, e) =>
            {
                switch(e.ButtonId)
                {
                    case OptionsKey:
                        var exchange = x.Exchange;
                        var optionsMenu = new OptionsMenu();
                        optionsMenu.Closed += (args, _) => x.Attach(exchange);

                        x.Exchange.Attach(optionsMenu);
                        x.Detach();
                        break;
                    case QuitGameKey:
                        x.Raise(new QuitEvent());
                        break;
                }
            })
        );

        public MainMenu() : base(Handlers)
        {
            StringId S(SystemTextId id) => new StringId(AssetType.SystemText, 0, (int)id);
            var elements = new List<IUiElement>
            {
                new Padding(0,2),
                new Header(S(SystemTextId.MainMenu_MainMenu)),
                new Divider(CommonColor.Yellow3),
                new Padding(0,2),
                new Button(ContinueKey, S(SystemTextId.MainMenu_ContinueGame)),
                new Padding(0,4),
                new Button(NewGameKey, S(SystemTextId.MainMenu_NewGame)),
                new Button(LoadGameKey, S(SystemTextId.MainMenu_LoadGame)),
                new Button(SaveGameKey, S(SystemTextId.MainMenu_SaveGame)),
                new Padding(0,4),
                new Button(OptionsKey, S(SystemTextId.MainMenu_Options)),
                new Button(ViewIntroKey, S(SystemTextId.MainMenu_ViewIntro)),
                new Button(CreditsKey, S(SystemTextId.MainMenu_Credits)),
                new Padding(0,3),
                new Button(QuitGameKey, S(SystemTextId.MainMenu_QuitGame)),
                new Padding(0,2),
            };
            var stack = new VerticalStack(elements);
            Children.Add(new Frame(stack));
        }

        protected override void Subscribed()
        {
            var layout = Exchange.Resolve<ILayoutManager>();
            layout.Add(this, DialogPositioning.Center);
        }
    }
}
