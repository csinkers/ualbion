using System.Collections.Generic;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Menus
{
    public class MainMenu : Dialog
    {
        public MainMenu() : base(DialogPositioning.Center)
        {
            On<CloseWindowEvent>(e => Raise(new PopSceneEvent()));
        }

        protected override void Subscribed()
        {
            RemoveAllChildren();

            var state = Resolve<IGameState>();
            var elements = new List<IUiElement>
            {
                new Spacing(0, 2),
                new HorizontalStack(new Spacing(5, 0), new Header(SystemTextId.MainMenu_MainMenu.ToId()), new Spacing(5, 0)),
                new Divider(CommonColor.Yellow3),
                new Spacing(0, 2),
            };

            if (state.Loaded)
            {
                elements.AddRange(new IUiElement[]
                {
                    new Button(SystemTextId.MainMenu_ContinueGame.ToId(), Continue),
                    new Spacing(0, 4),
                });
            }

            elements.AddRange(new IUiElement[]
            {
                new Button(SystemTextId.MainMenu_NewGame.ToId(), NewGame),
                new Button(SystemTextId.MainMenu_LoadGame.ToId(), LoadGame),
            });

            if (state.Loaded)
                elements.Add(new Button(SystemTextId.MainMenu_SaveGame.ToId(), SaveGame));

            elements.AddRange(new IUiElement[]
            {
                new Spacing(0,4),
                new Button(SystemTextId.MainMenu_Options.ToId(), Options),
                new Button(SystemTextId.MainMenu_ViewIntro.ToId(), ViewIntro),
                new Button(SystemTextId.MainMenu_Credits.ToId(), Credits),
                new Spacing(0,3),
                new Button(SystemTextId.MainMenu_QuitGame.ToId(), QuitGame),
                new Spacing(0,2),
            });

            var stack = new VerticalStack(elements);
            AttachChild(new DialogFrame(stack));
        }

        void Continue()
        {
            Raise(new PopSceneEvent());
        }

        void NewGame()
        {
            var yesNoDialog = new YesNoMessageBox(SystemTextId.MainMenu_DoYouReallyWantToStartANewGame.ToId());
            var exchange = Exchange;
            yesNoDialog.Closed += (args, _) =>
            {
                Attach(exchange);
                if (yesNoDialog.Result)
                    Raise(new NewGameEvent(
                        MapDataId.Toronto2DGesamtkarteSpielbeginn, 30, 75));
            };
            Exchange.Attach(yesNoDialog);
            Detach();
        }

        void LoadGame()
        {
            var menu = new PickSaveSlotMenu(false, SystemTextId.MainMenu_WhichSavedGameDoYouWantToLoad.ToId(), 1);
            var exchange = Exchange;
            menu.Closed += (args, filename) =>
            {
                Attach(exchange);
                if (filename != null)
                    Raise(new LoadGameEvent(filename));
            };
            Exchange.Attach(menu);
            Detach();
        }

        void SaveGame()
        {
            var menu = new PickSaveSlotMenu(true, SystemTextId.MainMenu_SaveOnWhichPosition.ToId(), 1);
            var exchange = Exchange;
            menu.Closed += (args, _) =>
            {
                Attach(exchange);
                // TODO: Prompt user for new save name
                // Raise(new SaveGameEvent(filename, name));
            };
            Exchange.Attach(menu);
            Detach();
        }

        void Options()
        {
            var optionsMenu = new OptionsMenu();
            var exchange = Exchange;
            optionsMenu.Closed += (args, _) => Attach(exchange);
            Exchange.Attach(optionsMenu);
            Detach();
        }

        void ViewIntro() { }
        void Credits() { }

        void QuitGame()
        {
            Raise(new QuitEvent());
        }

    }
}
