using System;
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

            if (ImGui.InputText("##" + name, buf, (uint)buf.Length))
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