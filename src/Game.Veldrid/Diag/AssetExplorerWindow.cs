﻿using System;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core.Veldrid;

namespace UAlbion.Game.Veldrid.Diag;

public class AssetExplorerWindow : Component, IImGuiWindow
{
    readonly byte[] _filterBuf = new byte[256];
    string _filter = "";
    AssetId _selected = AssetId.None;
    AssetId _hovered = AssetId.None;
    AssetViewerWindow _lastViewer;
    public string Name { get; }
    public AssetExplorerWindow(string name) => Name = name;
    public void Draw()
    {
        bool open = true;
        ImGui.Begin(Name, ref open);

        if (ImGui.InputText("Filter", _filterBuf, (uint)_filterBuf.Length))
            _filter = ImGuiUtil.GetString(_filterBuf);

        if (ImGui.Button("Open Viewer"))
        {
            var manager = Resolve<IImGuiManager>();
            var id = manager.GetNextWindowId();
            var name = $"AssetViewer##{id}";
            _lastViewer = new AssetViewerWindow(name);
            manager.AddWindow(_lastViewer);
        }

        if (_lastViewer != null)
            _lastViewer.Id = _selected;

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
                        _selected = id;
                    if (ImGui.IsItemHovered())
                        _hovered = id;
                }
            }

            if (nodeOpen)
                ImGui.TreePop();
        }
        ImGui.End();

        if (!open)
            Remove();
    }
}