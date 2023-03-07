using System.Collections.Generic;
using UAlbion.Core.Events;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Menus;

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
            new HorizontalStacker(new Spacing(5, 0), new BoldHeader((TextId)Base.SystemText.MainMenu_MainMenu), new Spacing(5, 0)),
            new Divider(CommonColor.Yellow3),
            new Spacing(0, 2),
        };

        if (state.Loaded)
        {
            elements.AddRange(new IUiElement[]
            {
                new Button(Base.SystemText.MainMenu_ContinueGame).OnClick(Continue),
                new Spacing(0, 4),
            });
        }

        elements.AddRange(new IUiElement[]
        {
            new Button(Base.SystemText.MainMenu_NewGame).OnClick(NewGame),
            new Button(Base.SystemText.MainMenu_LoadGame).OnClick(LoadGame),
        });

        if (state.Loaded)
            elements.Add(new Button(Base.SystemText.MainMenu_SaveGame).OnClick(SaveGame));

        elements.AddRange(new IUiElement[]
        {
            new Spacing(0,4),
            new Button(Base.SystemText.MainMenu_Options).OnClick(Options),
            new Button(Base.SystemText.MainMenu_ViewIntro),
            new Button(Base.SystemText.MainMenu_Credits),
            new Spacing(0,3),
            new Button(Base.SystemText.MainMenu_QuitGame).OnClick(QuitGame),
            new Spacing(0,2),
        });

        var stack = new VerticalStacker(elements);
        AttachChild(new DialogFrame(stack));
    }

    void Continue()
    {
        Raise(new PopSceneEvent());
    }

    void NewGame()
    {
        var e = new YesNoPromptEvent((TextId)Base.SystemText.MainMenu_DoYouReallyWantToStartANewGame);
        var exchange = Exchange;
        RaiseAsync(e, response =>
        {
            Attach(exchange);
            if (response)
                Raise(new NewGameEvent(Base.Map.TorontoBegin, 31, 76)); // TODO: Move this to config?
        });
        Detach();
    }

    void LoadGame()
    {
        var menu = new PickSaveSlotMenu(false, (TextId)Base.SystemText.MainMenu_WhichSavedGameDoYouWantToLoad, 1);
        var exchange = Exchange;
        menu.Closed += (args, id) =>
        {
            Attach(exchange);
            if (id.HasValue)
                Raise(new LoadGameEvent(id.Value));
        };
        Exchange.Attach(menu);
        Detach();
    }

    void SaveGame()
    {
        var menu = new PickSaveSlotMenu(true, (TextId)Base.SystemText.MainMenu_SaveOnWhichPosition, 1);
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

    void QuitGame()
    {
        Raise(new QuitEvent());
    }
}