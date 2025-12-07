using System;
using System.Linq;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Diag;

namespace UAlbion.Core.Veldrid.Reflection;

public class ReflectorSettingsWindow : Component, IImGuiWindow
{
    public const string NamePrefix = "Reflector Settings";

    static readonly DiagEditStyle[] Styles = [.. typeof(DiagEditStyle).GetEnumValues().OfType<DiagEditStyle>()];
    static readonly string[] StyleNames = [.. Styles.Select(x => x.ToString())];

    public string Name => NamePrefix;

    public ImGuiWindowDrawResult Draw()
    {
        bool open = true;
        ImGui.Begin(Name, ref open);
        DrawInner();
        ImGui.End();
        return open ? ImGuiWindowDrawResult.None : ImGuiWindowDrawResult.Closed;
    }

    void DrawInner()
    {
        var reflectorManager = Resolve<ReflectorManager>();
        var target = reflectorManager.EditTarget;
        if (target == null)
            return;

        ImGui.Text($"Editing {target.ParentType}.{target.Name}");

        if (target.Options != null && ImGui.Button("Reset to Defaults"))
            target.Options = null;

        if (target.Options == null && ImGui.Button("Override Defaults"))
            target.Options = new DiagEditAttribute { Style = DiagEditStyle.Label };

        if (ImGui.Button("Save Overrides"))
            reflectorManager.SaveOverrides();

        var options = target.Options;
        if (options != null)
        {
            int style = (int)options.Style;
            if (ImGui.Combo("Style", ref style, StyleNames, StyleNames.Length))
                options.Style = Styles[style];

            EditValue(false, target, static x => x.Options.Min, static (x, v) => x.Options.Min = v);
            EditValue(true, target, static x => x.Options.Max, static (x, v) => x.Options.Max = v);

            // target.Options.Min MinProperty
            // target.Options.Max MaxProperty
            // target.Options.MaxLength
        }
    }

    static void EditValue(
        bool isMax,
        ReflectorMetadata target,
        Func<ReflectorMetadata, object> getter,
        Action<ReflectorMetadata, object> setter)
    {
        var label = isMax ? "Max" : "Min";
        var rawValue = getter(target);

        if (target.ValueType == typeof(int))
        {
            if (rawValue is not int intValue)
            {
                ImGui.Text(label);
                ImGui.SameLine();

                if (ImGui.Button("Set"))
                    setter(target, isMax ? int.MaxValue : int.MinValue);
                return;
            }

            if (ImGui.DragInt(label, ref intValue))
                setter(target, intValue);
        }
        else if (target.ValueType == typeof(float))
        {
            if (rawValue is not float floatValue)
            {
                ImGui.Text(label);
                ImGui.SameLine();

                if (ImGui.Button("Set"))
                    setter(target, isMax ? float.MaxValue : float.MinValue);
                return;
            }

            if (ImGui.DragFloat(label, ref floatValue))
                setter(target, floatValue);
        }

        ImGui.SameLine();
        if (ImGui.Button("Clear##Clear" + label))
            setter(target, null);
    }
}