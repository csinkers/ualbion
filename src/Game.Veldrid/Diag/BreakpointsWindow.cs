using System;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid;

namespace UAlbion.Game.Veldrid.Diag;

public class BreakpointsWindow : Component, IImGuiWindow
{
    readonly DiagNewBreakpoint _newBreakpoint;
    readonly string _name;

    int _currentBreakpointIndex;
    string[] _breakpointNames = Array.Empty<string>();

    public BreakpointsWindow(string name)
    {
        _name = name;
        _newBreakpoint = AttachChild(new DiagNewBreakpoint());
    }

    public void Draw()
    {
        bool open = true;
        ImGui.Begin(_name, ref open);
        ImGui.TextUnformatted(Context.ToString());

        var chainManager = Resolve<IEventManager>();

        if (ImGui.Button("Add BP"))
            ImGui.OpenPopup("New Breakpoint");

        _newBreakpoint.Render();

        if (ImGui.Button("Del BP") && _currentBreakpointIndex < chainManager.Breakpoints.Count)
            chainManager.RemoveBreakpoint(_currentBreakpointIndex);

        if (_breakpointNames.Length != chainManager.Breakpoints.Count)
            _breakpointNames = new string[chainManager.Breakpoints.Count];

        for (int i = 0; i < _breakpointNames.Length; i++)
            _breakpointNames[i] = chainManager.Breakpoints[i].ToString();

        if (ImGui.ListBox("Breakpoints", ref _currentBreakpointIndex, _breakpointNames, _breakpointNames.Length))
        {
        }

        ImGui.End();

        if (!open)
            Remove();
    }
}