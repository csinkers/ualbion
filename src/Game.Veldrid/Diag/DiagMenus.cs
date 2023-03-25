using ImGuiNET;
using UAlbion.Core.Veldrid;

namespace UAlbion.Game.Veldrid.Diag;

public static class DiagMenus
{
    public static void Draw(IImGuiManager manager)
    {
        if (!ImGui.BeginMainMenuBar())
            return;

        if (ImGui.BeginMenu("Windows"))
        {
            if (ImGui.BeginMenu("Debug"))
            {
                if (ImGui.MenuItem("Breakpoints")) manager.AddWindow(new BreakpointsWindow(manager.GetNextWindowId()));
                // if (ImGui.MenuItem("Code")) manager.AddWindow(new CodeWindow(manager.GetNextWindowId()));
                if (ImGui.MenuItem("Threads")) manager.AddWindow(new ThreadsWindow(manager.GetNextWindowId()));
                if (ImGui.MenuItem("Watch")) manager.AddWindow(new WatchWindow(manager.GetNextWindowId()));
                ImGui.EndMenu();
            }

            if (ImGui.MenuItem("Asset Explorer")) manager.AddWindow(new AssetExplorerWindow(manager.GetNextWindowId()));
            if (ImGui.MenuItem("Console")) manager.AddWindow(new ImGuiConsoleLogger(manager.GetNextWindowId()));
            if (ImGui.MenuItem("Demo Window")) manager.AddWindow(new DemoWindow(manager.GetNextWindowId()));
            if (ImGui.MenuItem("Inspector Demo")) manager.AddWindow(new InspectorDemoWindow(manager.GetNextWindowId()));
            if (ImGui.MenuItem("Inspector")) manager.AddWindow(new InspectorWindow(manager.GetNextWindowId()));
            // if (ImGui.MenuItem("Profiler")) manager.AddWindow(new ProfilerWindow(manager.GetNextWindowId()));
            if (ImGui.MenuItem("Positions")) manager.AddWindow(new PositionsWindow(manager.GetNextWindowId()));
            if (ImGui.MenuItem("Settings")) manager.AddWindow(new SettingsWindow(manager.GetNextWindowId()));
            if (ImGui.MenuItem("Stats")) manager.AddWindow(new StatsWindow(manager.GetNextWindowId()));

            ImGui.EndMenu();
        }

        ImGui.EndMainMenuBar();
    }
}