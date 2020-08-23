using ImGuiNET;
using UAlbion.Core;
using UAlbion.Core.Events;

namespace UAlbion.Game.Veldrid.Editor
{
    public class EditorUi : Component
    {
        bool _showInspector;
        bool _showAssets;

        public EditorUi()
        {
            On<EngineUpdateEvent>(e => Render());
        }

        void Render()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                    ImGui.EndMenu();
                if (ImGui.BeginMenu("Edit"))
                    ImGui.EndMenu();
                if (ImGui.BeginMenu("View"))
                    ImGui.EndMenu();
                if (ImGui.BeginMenu("Window"))
                    ImGui.EndMenu();

                _showInspector = ImGui.Button("Inspector");
                _showAssets = ImGui.Button("Assets");

                ImGui.EndMainMenuBar();
            }

            if(_showInspector) ImGui.OpenPopup("Editor#Inspector");
            if(_showAssets) ImGui.OpenPopup("Editor#Assets");

            if (ImGui.BeginPopup("Editor#Inspector"))
                ImGui.EndPopup();

            if (ImGui.BeginPopup("Editor#Assets"))
                ImGui.EndPopup();

            // Left pane
            // Right pane
            // Central pane with tabs


                /* Tab types:
                - 2D Map editor
                    - Edit map
                    - Import / export tileset graphics
                    - Edit tile data, passability, layering etc
                    - Edit NPCs
                    - Edit scripts
                    - Edit map text
                - 3D Map editor
                    - General properties
                    - Edit layout
                    - Edit NPCs
                    - Edit scripts
                    - Edit map text
                    - Edit automap (add markers etc)
                - Labyrinth data editor
                    - General properties
                    - Edit object groups
                    - Edit objects
                    - Edit walls / floor / ceilings
                - Block library
                - Script editor
                - Character sheet editor
                - Edit chests / merchants / character sheet inventories
                - Monster group editor
                - Item editor
                - Image viewer, import / export (fixed size, header size, per-frame size etc)
                - Spell editor
                - Save editor
                - Sound player / import / export
                */
        }
    }
}
