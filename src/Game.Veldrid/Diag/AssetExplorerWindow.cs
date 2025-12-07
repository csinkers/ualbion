using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core.Veldrid.Diag;

namespace UAlbion.Game.Veldrid.Diag;

public class AssetExplorerWindow : Component, IImGuiWindow
{
    readonly byte[] _filterBuf = new byte[256];
    string _filter = "";
    AssetId _selected = AssetId.None;
    AssetId _hovered = AssetId.None;
    AssetViewerWindow _lastViewer;
    bool _init = true;
    public string Name { get; }
    public AssetExplorerWindow(string name) => Name = name;

    public ImGuiWindowDrawResult Draw()
    {
        bool open = true;
        ImGui.Begin(Name, ref open);

        if (ImGui.InputText("Filter", _filterBuf, (uint)_filterBuf.Length))
            _filter = ImGuiUtil.GetString(_filterBuf);

        if (_init)
        {
            var manager = Resolve<IImGuiManager>();
            _lastViewer = manager.FindWindows("Asset Viewer").OfType<AssetViewerWindow>().FirstOrDefault();
            _init = false;
        }

        if (ImGui.Button("Open Viewer"))
        {
            var manager = Resolve<IImGuiManager>();
            _lastViewer = manager.FindWindows("Asset Viewer").OfType<AssetViewerWindow>().FirstOrDefault();
            if (_lastViewer == null)
            {
                var id = manager.GetNextWindowId();
                var name = $"Asset Viewer##{id}";
                _lastViewer = new AssetViewerWindow(name);

                manager.AddWindow(_lastViewer);
            }
        }

        bool selectionChanged = false;
        foreach (var type in Enum.GetValues<AssetType>())
        {
            bool shown = false;
            bool nodeOpen = false;
            foreach (var id in AssetMapping.Global.EnumerateAssetsOfType(type))
            {
                var name = id.ToString();
                if (_filter.Length > 0 && !name.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!shown)
                {
                    shown = true;
                    nodeOpen = ImGui.TreeNode(type.ToString());
                }

                if (nodeOpen)
                {
                    if (id == _selected)
                        ImGui.TextColored(new Vector4(0.3f, 1.0f, 0.3f, 1.0f), name);
                    else if (id == _hovered)
                        ImGui.TextColored(new Vector4(0.3f, 0.7f, 0.3f, 1.0f), name);
                    else
                        ImGui.Text(name);

                    if (ImGui.IsItemClicked())
                    {
                        _selected = id;
                        selectionChanged = true;
                    }

                    if (ImGui.IsItemHovered())
                        _hovered = id;
                }
            }

            if (nodeOpen)
                ImGui.TreePop();
        }
        ImGui.End();

        if (selectionChanged && _lastViewer != null)
            _lastViewer.Id = _selected;

        return open ? ImGuiWindowDrawResult.None : ImGuiWindowDrawResult.Closed;
    }
}