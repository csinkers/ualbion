using ImGuiNET;

namespace UAlbion.Core.Veldrid.Reflection;

sealed class BoolReflector : IReflector
{
    BoolReflector() { }
    public static BoolReflector Instance { get; } = new();

    public void Reflect(in ReflectorState state)
    {
        var value = (bool)state.Target;
        var description = ReflectorUtil.NameText(state);
        ImGui.Indent();
        ImGui.TextWrapped(description);
        ImGui.SameLine();

        if (state.Meta.Options != null)
        {
            if (ImGui.Checkbox("##" + description, ref value))
                state.Meta.Setter(state, value);
        }
        else
            ImGui.TextWrapped(value.ToString());

        ImGui.Unindent();
    }
}