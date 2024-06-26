﻿using System;
using System.Text;
using ImGuiNET;

namespace UAlbion.Core.Veldrid.Reflection;

sealed class StringReflector : IReflector
{
    StringReflector() { }
    public static StringReflector Instance { get; } = new();

    public void Reflect(in ReflectorState state)
    {
        var name = ReflectorUtil.NameText(state);
        ImGui.Indent();
        ImGui.TextWrapped(name);
        ImGui.SameLine();

        if (state.Meta?.Options != null)
        {
            var buf = ReflectorUtil.GetAuxiliaryState(
                state,
                "StringState",
                s =>
                {
                    int len = 1024;
                    if (s.Meta?.Options?.MaxLength != null)
                        len = s.Meta.Options.MaxLength;

                    var buf = new byte[len];
                    var str = (string)s.Target;
                    var charSpan = str.ToCharArray().AsSpan();
                    Encoding.UTF8.GetBytes(charSpan, buf);
                    return buf;
                }
            );

            if (ImGui.InputText(name, buf, (uint)buf.Length))
            {
                var str = ImGuiUtil.GetString(buf);
                state.Meta.Setter(state, str);
            }
        }
        else
        {
            var value = $"\"{((string)state.Target)?.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
            value = CoreUtil.WordWrap(value, 120);
            ImGui.TextWrapped(value);
        }

        ImGui.Unindent();

    }
}
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

        if (state.Meta?.Options != null)
        {
            if (ImGui.Checkbox("##" + description, ref value))
                state.Meta.Setter(state, value);
        }
        else
            ImGui.TextWrapped(value.ToString());

        ImGui.Unindent();
    }
}
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
