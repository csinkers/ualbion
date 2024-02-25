using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Game.Gui;

namespace UAlbion.Game.Veldrid.Diag;

public class LayoutWindow : Component, IImGuiWindow
{
    public string Name { get; }
    public LayoutWindow(string name) => Name = name;

    public void Draw()
    {
        bool open = true;
        ImGui.Begin(Name, ref open);

        var manager = Resolve<ILayoutManager>();
        var engineFlags = ReadVar(V.Core.User.EngineFlags);
        bool suppressLayout = (engineFlags & EngineFlags.SuppressLayout) != 0;
        if (ImGui.Checkbox("Suppress Layout", ref suppressLayout))
        {
            Raise(new EngineFlagEvent(
                suppressLayout ? FlagOperation.Set : FlagOperation.Clear,
                EngineFlags.SuppressLayout));
        }

        if (ImGui.Button("Layout"))
        {
            manager.GetLayout();
            Raise(new LayoutEvent());
        }

        var layout = manager.LastLayout;
        static void Aux(LayoutNode node)
        {
            var size = node.Element?.GetSize() ?? Vector2.Zero;
            var id = $"{node.Order,4}";
            var label = $"{id} ({node.Extents.X,3}, {node.Extents.Y,3}, {node.Extents.Width,3}, {node.Extents.Height,3}) <{size.X,3}, {size.Y,3}> {node.Element}";
            if (node.Children.Count == 0)
            {
                ImGui.Indent();
                ImGui.TextUnformatted(label);
                ImGui.Unindent();
            }
            else if (ImGui.TreeNode($"###{id}"))
            {
                ImGui.SameLine();
                ImGui.TextUnformatted(label);
                foreach (var child in node.Children)
                    Aux(child);

                ImGui.TreePop();
            }
            else
            {
                ImGui.SameLine();
                ImGui.TextUnformatted(label);
            }
        }

        if (layout != null)
        {
            Aux(layout);
        }

        ImGui.End();

        if (!open)
            Remove();
    }
}