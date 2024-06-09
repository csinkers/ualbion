using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid;

namespace UAlbion.Game.Veldrid.Diag;

public class ThreadsWindow : Component, IImGuiWindow
{

    int _currentContextIndex;
    string[] _contextNames = [];

    public string Name { get; }
    public ThreadsWindow(string name) => Name = name;

    public void Draw()
    {
        bool open = true;
        ImGui.Begin(Name, ref open);
        ImGui.TextUnformatted(Context.ToString());

        var chainManager = Resolve<IEventManager>();

        if (_contextNames.Length != chainManager.Contexts.Count)
            _contextNames = new string[chainManager.Contexts.Count];

        for (int i = 0; i < _contextNames.Length; i++)
            _contextNames[i] = chainManager.Contexts[i].ToString();

        ImGui.ListBox("Active Contexts", ref _currentContextIndex, _contextNames, _contextNames.Length);
        ImGui.End();

        if (!open)
            Remove();
    }
}