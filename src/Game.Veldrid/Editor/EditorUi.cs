using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Veldrid.Editor
{
    public abstract class AssetEditor : Component
    {
        public abstract void Render();
    }

    public class MapEditor : AssetEditor
    {
        readonly string _name;

        public MapEditor(string name)
        {
            _name = name;
        }

        public override void Render()
        {
            if (!ImGui.Begin(_name)) return;
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.MenuItem("Show Overlay", true)) { }
                    if (ImGui.MenuItem("Show Underlay", true)) { }
                    if (ImGui.MenuItem("Show NPCs", true)) { }
                    if (ImGui.MenuItem("Show NPC paths", true)) { }
                    if (ImGui.MenuItem("Show trigger zones", true)) { }
                        

                }

                ImGui.EndMenuBar();
            }
            ImGui.End();
        }
    }

    public class CharacterEditor : AssetEditor
    {
        readonly string _name;
        readonly AssetKey _key;

        public CharacterEditor(string name, AssetKey key)
        {
            _name = name;
            _key = key;
        }

        public override void Render()
        {
            if (!ImGui.Begin(_name)) return;
            if (ImGui.BeginMenuBar())
            {

                ImGui.EndMenuBar();
            }
            ImGui.End();
        }
    }

    public class EditorUi : Component
    {
        bool _showInspector;
        bool _showAssets;

        readonly Dictionary<AssetType, AssetKey[]> _allKeys = new Dictionary<AssetType, AssetKey[]>();

        public EditorUi()
        {
            On<EngineUpdateEvent>(e =>
            {
                RenderMenus();
                RenderAssetPicker();
                RenderChildren();
            });
            AttachChild(new MapEditor("Map Editor"));
            AttachChild(new CharacterEditor("Tom", PartyCharacterId.Tom.ToAssetId()));
        }

        void ReloadAssetKeys()
        {
            var raw = Resolve<IRawAssetManager>();
            _allKeys.Clear();
            foreach (var assetType in Enum.GetValues(typeof(AssetType)).OfType<AssetType>())
                _allKeys[assetType] = raw.EnumerateAssets(assetType).ToArray();
        }

        void RenderChildren()
        {
            foreach (var child in Children.OfType<AssetEditor>())
                child.Render();
        }

        void RenderAssetPicker()
        {
            if(_allKeys.Count == 0)
                ReloadAssetKeys();

            // TODO: Filter textbox w/ keyboard shortcuts
            ImGui.SetWindowSize(new Vector2(300, 200), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Assets"))
            {
                foreach (var assetType in Enum.GetValues(typeof(AssetType)).OfType<AssetType>())
                {
                    // TODO: Coloured / grouped by asset type?
                    if (ImGui.TreeNode(assetType.ToString()))
                    {
                        // TODO: Mark modified/overridden assets, assets with unsaved changes, zero-length assets, missing assets etc
                        foreach(var key in _allKeys[assetType])
                            ImGui.Text(key.ToString());
                        ImGui.TreePop();
                    }
                }

                ImGui.End();
            }
        }

        void RenderMenus()
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
