using System;
using ImGuiNET;

namespace UAlbion.Game.Veldrid.Diag;

static class EnumReflector
{
    public static Reflector Build(Type type)
    {
        var typeName = ReflectorUtil.BuildTypeName(type);
        return (in ReflectorState state) =>
        {
            var description = ReflectorUtil.Describe(state, typeName, state.Target);
            ImGui.Indent();
            ImGui.TextWrapped(description);
            ImGui.Unindent();
        };
    }
}