using System;
using ImGuiNET;
using UAlbion.Formats;

namespace UAlbion.Game.Veldrid.Diag;

public static class ValueReflectorBuilder
{
    public static Reflector Build(string typeName)
    {
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

    public static Reflector Build(string typeName, Func<object, string> toString)
    {
        return (in ReflectorState state) =>
        {
            var value = toString(state.Target);
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