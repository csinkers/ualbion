using System;
using ImGuiNET;
using UAlbion.Core;

namespace UAlbion.Game.Gui
{
    public enum DrawLayer
    {
        Background,
        Tiles,
        Objects,
        Characters,
        Effects
    }

    internal class MainMenu : IComponent
    {
        // Background = AssetType.Picture

        Action _menuFunc;

        public MainMenu()
        {
            _menuFunc = PrimaryMenu;
        }


        public void Attach(EventExchange exchange)
        {
            exchange.Subscribe<EngineUpdateEvent>(this);
        }

        public void Receive(IEvent @event)
        {
            switch (@event)
            {
                case EngineUpdateEvent e:
                    _menuFunc();
                    break;
            }
        }

        void PrimaryMenu()
        {
            bool gameInProgress = false;
            const int savedGames = 0;
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
            {
                // TODO
            }
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
