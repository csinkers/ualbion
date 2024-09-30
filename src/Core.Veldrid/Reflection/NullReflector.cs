using ImGuiNET;

namespace UAlbion.Core.Veldrid.Reflection;

sealed class NullReflector : IReflector
{
    NullReflector() { }
    public static NullReflector Instance { get; } = new();

    public void Reflect(in ReflectorState state)
    {
        var description = ReflectorUtil.Describe(state, "null", "null");
        ImGui.Indent();
        ImGui.TextUnformatted(description);
        ImGui.Unindent();
    }
}