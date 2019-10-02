using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui
{
    public class MainMenu : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<MainMenu, EngineUpdateEvent>((x, _) => x._menuFunc()),
            new Handler<MainMenu, SubscribedEvent>((x, _) => x.Rebuild())
        };

        Action _menuFunc;
        int _width = 9;
        int _height = 14;

        public MainMenu() : base(Handlers) { _menuFunc = PrimaryMenu; }

        void Rebuild()
        {
            var assets = Exchange.Resolve<IAssetManager>();
            var settings = Exchange.Resolve<ISettings>();
            var window = Exchange.Resolve<IWindowState>();
            string S(SystemTextId id) => assets.LoadString(id, settings.Language);

            var frame = new Frame(140, 40, 79, 112);
            Exchange.Attach(frame);
            var origin = window.UiToScreen(130, 30);
            var grid = new Vector2(0, -window.GuiScale * 12) / window.Size;

            var header = new Header(origin, _width - 2, 1, S(SystemTextId.MainMenu_MainMenu));
            var divider = new Divider();
            var buttons = new[]
            {
                new Button(origin + grid*1, 16*(_width-2), 16, S(SystemTextId.MainMenu_ContinueGame)),
                new Button(origin + grid*2, 16*(_width-2), 16, S(SystemTextId.MainMenu_NewGame)),
                new Button(origin + grid*3, 16*(_width-2), 16, S(SystemTextId.MainMenu_LoadGame)),
                new Button(origin + grid*4, 16*(_width-2), 16, S(SystemTextId.MainMenu_SaveGame)),
                new Button(origin + grid*5, 16*(_width-2), 16, S(SystemTextId.MainMenu_Options)),
                new Button(origin + grid*6, 16*(_width-2), 16, S(SystemTextId.MainMenu_ViewIntro)),
                new Button(origin + grid*7, 16*(_width-2), 16, S(SystemTextId.MainMenu_Credits)),
                new Button(origin + grid*8, 16*(_width-2), 16, S(SystemTextId.MainMenu_QuitGame)),
            };

            Exchange.Attach(header);
            foreach (var button in buttons)
                Exchange.Attach(button);
        }

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
