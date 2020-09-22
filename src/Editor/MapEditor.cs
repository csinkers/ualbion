using System;
using ImGuiNET;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Editor
{
    public class FlatMapEditor : AssetEditor
    {
        readonly string _name;
        readonly MapData2D _map;

        public FlatMapEditor(string name, MapData2D map): base(map)
        {
            _name = name;
            _map = map ?? throw new ArgumentNullException(nameof(map));
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
}