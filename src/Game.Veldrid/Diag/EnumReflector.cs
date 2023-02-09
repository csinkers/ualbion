using System;
using ImGuiNET;
using UAlbion.Formats;

namespace UAlbion.Game.Veldrid.Diag;

static class EnumReflector
{
    public static Reflector Build(Type type)
    {
        var typeName = ReflectorManager.BuildTypeName(type);
        return (in ReflectorState state) =>
        {
            var value = state.Target;
            var description =
                state.Meta?.Name == null
                    ? $"{value} ({typeName})"
                    : $"{state.Meta.Name}: {value} ({typeName})";

            description = FormatUtil.WordWrap(description, 120);
            ImGui.Indent();
            ImGui.TextWrapped(description);
            ImGui.Unindent();
        };
    }
}