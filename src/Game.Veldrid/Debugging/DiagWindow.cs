using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Debugging;

namespace UAlbion.Game.Veldrid.Debugging;

[Event("hide_diag_window", "Hide the diagnostics window", "hdw")]
public class HideDiagWindowEvent : Event { }

/*
TODO: Event debugger

| [x] Break on next
| [Step Over] [Step In] [Step Out]
| [Tabs for active scripts]
|#if (prompt_user bla) {
|     map_text 231 ; Some text
| } else {
|     teleport 200 60 50
| }
|----------------------------------------------------
| Watch Window    |       | Call Stack
| Switch.123      | true  | current script context
| Ticker.10       | 5     | previous etc
| - NpcSheet.Sira |       |
| |-- Name        | Sira  |
| |-- Inventory   |       |
| ||-- Gold       | 23.5  |
| ||+- etc        |       |

*/

public class DiagWindow : Container
{
    readonly DiagDebugger _debugger;
    readonly DiagInspector _inspector;
    bool _visible;

    public DiagWindow() : base("DiagWindow")
    {
        _debugger = AttachChild(new DiagDebugger());
        _inspector = AttachChild(new DiagInspector());
        On<EngineUpdateEvent>(_ => RenderDialog());
        On<HideDiagWindowEvent>(_ => _visible = false);
        On<ShowDebugInfoEvent>(e => _visible = true);
    }

    protected override bool AddingChild(IComponent child)
    {
        if (child is IDebugBehaviour behaviour)
            foreach (var type in behaviour.HandledTypes)
                _inspector.AddBehaviour(type, behaviour.Handle);

        return true;
    }

    void RenderDialog()
    {
        if (!_visible)
            return;

        var window = Resolve<IWindowManager>();

        ImGui.Begin("Diag");
        ImGui.SetWindowPos(new Vector2(2 * window.PixelWidth / 3.0f, 0), ImGuiCond.FirstUseEver);
        ImGui.SetWindowSize(new Vector2(window.PixelWidth / 3.0f, window.PixelHeight), ImGuiCond.FirstUseEver);

        if (ImGui.Button("Close"))
        {
            _visible = false;
            ImGui.EndChild();
            ImGui.End();
            return;
        }

        ImGui.BeginTabBar("Tabs");

        DrawInspectorTab();
        DrawDebuggerTab();

        ImGui.EndTabBar();
        ImGui.End();

        /*
        Window: Begin & End
        Menus: BeginMenuBar, MenuItem, EndMenuBar
        Colours: ColorEdit4
        Graph: PlotLines
        Text: Text, TextColored
        ScrollBox: BeginChild, EndChild
        */
    }

    void DrawInspectorTab()
    {
        if (!ImGui.BeginTabItem("Inspector"))
            return;

        _inspector.Render();
        ImGui.EndTabItem();
    }

    void DrawDebuggerTab()
    {
        if (!ImGui.BeginTabItem("Debugger"))
            return;

        _debugger.Render();
        ImGui.EndTabItem();
    }
}