using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class MainMenu : Component, IUiElement
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<MainMenu, EngineUpdateEvent>((x, _) => x._menuFunc()),
        };

        readonly Frame _frame;
        Action _menuFunc;

        public MainMenu() : base(Handlers)
        {
            _menuFunc = PrimaryMenu;
            StringId S(SystemTextId id) => new StringId(AssetType.SystemText, 0, (int)id);

            // Exchange.Attach(frame);
            // var origin = window.UiToScreen(130, 30);
            // var grid = new Vector2(0, -window.GuiScale * 12) / window.Size;
            var elements = new List<IUiElement>
            {
                new Header(S(SystemTextId.MainMenu_MainMenu)),
                new Divider(MetaFontId.FontColor.Yellow),
                new Padding(0,2),
                new Button(S(SystemTextId.MainMenu_ContinueGame)),
                new Padding(0,4),
                new Button(S(SystemTextId.MainMenu_NewGame)),
                new Button(S(SystemTextId.MainMenu_LoadGame)),
                new Button(S(SystemTextId.MainMenu_SaveGame)),
                new Padding(0,4),
                new Button(S(SystemTextId.MainMenu_Options)),
                new Button(S(SystemTextId.MainMenu_ViewIntro)),
                new Button(S(SystemTextId.MainMenu_Credits)),
                new Padding(0,3),
                new Button(S(SystemTextId.MainMenu_QuitGame)),
            };
            var stack = new VerticalStack(elements);
            _frame = new Frame(new IUiElement[] { stack }); //140, 40, 79, 112);
            Children.Add(_frame);
        }
        public IUiElement Parent => null;
        public Vector2 Size => _frame.Size;
        public void Render(Rectangle position, Action<IRenderable> addFunc) { }

        void PrimaryMenu()
        {
            bool gameInProgress = false;
            const int savedGames = 0;
            ImGui.Begin("Main Menu");
            if (savedGames > 0)
            {
                if (ImGui.Button("Continue game"))
                {
                }
            }

            if (ImGui.Button("New Game"))
            {
                // TODO
            }

            if (ImGui.Button("Load Game"))
                _menuFunc = LoadGameMenu;

            if (gameInProgress)
            {
                if (ImGui.Button("Save Game"))
                    _menuFunc = SaveGameMenu;
            }

            if (ImGui.Button("Options"))
                _menuFunc = OptionsMenu;

            if (ImGui.Button("View Intro"))
            {
                // TODO
            }

            if (ImGui.Button("Credits"))
            {
                // TODO
            }

            if (ImGui.Button("Quit"))
                Raise(new QuitEvent());

            ImGui.End();
        }

        void OptionsMenu()
        {
            int musicVolume = 64, fxVolume = 64, windowSize3d = 100, combatDetailLevel = 5, combatTextDelay = 10;
            ImGui.SliderInt("Music Volume", ref musicVolume, 0, 127);
            ImGui.SliderInt("Fx Volume", ref fxVolume, 0, 127);
            ImGui.SliderInt("3D Window Size", ref windowSize3d, 0, 100);
            ImGui.SliderInt("Combat Detail Level", ref combatDetailLevel, 1, 5);
            ImGui.SliderInt("Combat Text Delay", ref combatTextDelay, 1, 50);

            if (ImGui.Button("Back"))
                _menuFunc = PrimaryMenu;
        }

        void LoadGameMenu()
        {
            for (int i = 0; i < 10; i++)
            {
                if (ImGui.Button($"SaveName{i}"))
                {
                    // TODO
                }
            }

            if (ImGui.Button("Back"))
                _menuFunc = PrimaryMenu;
        }

        void SaveGameMenu()
        {
            for (int i = 0; i < 10; i++)
            {
                if (ImGui.Button($"SaveName{i}"))
                {
                    // TODO
                }
            }

            if (ImGui.Button("Back"))
                _menuFunc = PrimaryMenu;
        }
    }
}
