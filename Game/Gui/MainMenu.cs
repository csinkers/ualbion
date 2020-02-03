using System.Collections.Generic;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui
{
    public class MainMenu : Dialog
    {
        const string ContinueKey = "MainMenu.ContinueGame";
        const string NewGameKey = "MainMenu.NewGame";
        const string LoadGameKey = "MainMenu.LoadGame";
        const string SaveGameKey = "MainMenu.SaveGame";
        const string OptionsKey = "MainMenu.Options";
        const string ViewIntroKey = "MainMenu.ViewIntro";
        const string CreditsKey = "MainMenu.Credits";
        const string QuitGameKey = "MainMenu.QuitGame";
        static StringId S(SystemTextId id) => new StringId(AssetType.SystemText, 0, (int)id);

        static readonly HandlerSet Handlers = new HandlerSet(
            H<MainMenu, ButtonPressEvent>((x, e) => x.OnButton(e.ButtonId)),
            H<MainMenu, CloseDialogEvent>((x, e) => x.OnButton(ContinueKey))
        );

        public MainMenu() : base(Handlers, DialogPositioning.Center) { }

        void OnButton(string buttonId)
        {
            var exchange = Exchange;
            switch (buttonId)
            {
                case ContinueKey: Raise(new PopSceneEvent()); break;
                case NewGameKey:
                    var yesNoDialog = new YesNoMessageBox(S(SystemTextId.MainMenu_DoYouReallyWantToStartANewGame));
                    yesNoDialog.Closed += (args, _) =>
                    {
                        Attach(exchange);
                        if (yesNoDialog.Result)
                            Raise(new NewGameEvent());
                    };
                    Exchange.Attach(yesNoDialog);
                    Detach();
                    break;

                case OptionsKey:
                    var optionsMenu = new OptionsMenu();
                    optionsMenu.Closed += (args, _) => Attach(exchange);
                    Exchange.Attach(optionsMenu);
                    Detach();
                    break;

                case QuitGameKey:
                    Raise(new QuitEvent());
                    break;
            }
        }

        public override void Subscribed()
        {
            foreach (var child in Children)
                child.Detach();
            Children.Clear();

            var state = Resolve<IGameState>();
            var elements = new List<IUiElement>
            {
                new Padding(0, 2),
                new HorizontalStack(new Padding(5, 0), new Header(S(SystemTextId.MainMenu_MainMenu)), new Padding(5, 0)),
                new Divider(CommonColor.Yellow3),
                new Padding(0, 2),
            };

            if (state.Loaded)
            {
                elements.AddRange(new IUiElement[]
                {
                    new Button(ContinueKey, S(SystemTextId.MainMenu_ContinueGame)),
                    new Padding(0, 4),
                });
            }

            elements.AddRange(new IUiElement[]
            {
                new Button(NewGameKey, S(SystemTextId.MainMenu_NewGame)),
                new Button(LoadGameKey, S(SystemTextId.MainMenu_LoadGame)),
            });

            if (state.Loaded)
                elements.Add(new Button(SaveGameKey, S(SystemTextId.MainMenu_SaveGame)));

            elements.AddRange(new IUiElement[]
            {
                new Padding(0,4),
                new Button(OptionsKey, S(SystemTextId.MainMenu_Options)),
                new Button(ViewIntroKey, S(SystemTextId.MainMenu_ViewIntro)),
                new Button(CreditsKey, S(SystemTextId.MainMenu_Credits)),
                new Padding(0,3),
                new Button(QuitGameKey, S(SystemTextId.MainMenu_QuitGame)),
                new Padding(0,2),
            });

            var stack = new VerticalStack(elements);
            Children.Add(new DialogFrame(stack));

            foreach (var child in Children)
                child.Attach(Exchange);

            base.Subscribed();
        }
    }
}
