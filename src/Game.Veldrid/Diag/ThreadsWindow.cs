using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Diag;
using UAlbion.Core.Veldrid.Reflection;

namespace UAlbion.Game.Veldrid.Diag;

public class ThreadsWindow : Component, IImGuiWindow
{
    readonly StringCache<int> _stringCache = new();

    string[] _contextNames = [];

    public string Name { get; }
    public ThreadsWindow(string name) => Name = name;

    public ImGuiWindowDrawResult Draw()
    {
        bool open = true;
        ImGui.Begin(Name, ref open);
        ImGui.TextUnformatted(Context.ToString());

        var chainManager = Resolve<IEventManager>();

        if (_contextNames.Length != chainManager.Contexts.Count)
            _contextNames = new string[chainManager.Contexts.Count];

        for (int i = 0; i < _contextNames.Length; i++)
            _contextNames[i] = chainManager.Contexts[i].ToString();

        int currentContextIndex = chainManager.CurrentDebugContextIndex;
        if (ImGui.ListBox("Active Contexts", ref currentContextIndex, _contextNames, _contextNames.Length))
            chainManager.CurrentDebugContextIndex = currentContextIndex;

#if DEBUG
        ImGui.Text("Pending Async Tasks:");
        var reflectorManager = Resolve<ReflectorManager>();
        Tasks.EnumeratePendingTasks((_stringCache, reflectorManager), static (context, core) =>
        {
            var name = context._stringCache.Get(core.Id, 0, static (x, _) => $"Task{x}");
            context.reflectorManager.RenderNode(name, core);
        });
#endif

        ImGui.End();
        return open ? ImGuiWindowDrawResult.None : ImGuiWindowDrawResult.Closed;
    }
}