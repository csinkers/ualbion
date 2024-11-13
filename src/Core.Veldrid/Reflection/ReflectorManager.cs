using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;
using Vulkan.Xcb;
using Component = System.ComponentModel.Component;

namespace UAlbion.Core.Veldrid.Reflection;

public class ReflectorManager
{
    static readonly DiagEditStyle[] Styles =
        typeof(DiagEditStyle)
            .GetEnumValues()
            .OfType<DiagEditStyle>()
            .ToArray();

    static readonly string[] StyleNames = Styles.Select(x => x.ToString()).ToArray();

    readonly Reflector _nullReflector;
    readonly Dictionary<Type, Reflector> _reflectors = new();
    bool _editMode;

    public static ReflectorManager Instance { get; } = new();
    public bool IsEditMode => _editMode;
    public ReflectorMetadata EditTarget { get; set; }

    ReflectorManager()
    {
        // void Add<T>(string name) => _reflectors[typeof(T)] = new ValueReflector(name).Reflect;
        void Add2<T>(string name, Func<object, string> toString) => _reflectors[typeof(T)] = new ValueReflector(name, toString).Reflect;

        _nullReflector              = NullReflector.Instance.Reflect;
        _reflectors[typeof(bool)]   = BoolReflector.Instance.Reflect;
        _reflectors[typeof(string)] = StringReflector.Instance.Reflect;
        _reflectors[typeof(Vector3)] = Vec3Reflector.Instance.Reflect;
        _reflectors[typeof(Vector4)] = Vec4Reflector.Instance.Reflect;
        _reflectors[typeof(byte)]   = new IntReflector("byte",   x => (byte)x).Reflect;
        _reflectors[typeof(sbyte)]  = new IntReflector("sbyte",  x => (sbyte)x).Reflect;
        _reflectors[typeof(ushort)] = new IntReflector("ushort", x => (ushort)x).Reflect;
        _reflectors[typeof(short)]  = new IntReflector("short",  x => (short)x).Reflect;
        _reflectors[typeof(uint)]   = new IntReflector("uint",   x => (int)(uint)x).Reflect;
        _reflectors[typeof(int)]    = new IntReflector("int",    x => (int)x).Reflect;
        _reflectors[typeof(ulong)]  = new IntReflector("ulong",  x => (int)(ulong)x).Reflect;
        _reflectors[typeof(long)]   = new IntReflector("long",   x => (int)(long)x).Reflect;
        _reflectors[typeof(float)]  = new FloatReflector("float", x => (float)x).Reflect;
        _reflectors[typeof(double)] = new FloatReflector("double", x => (float)(double)x).Reflect;

        Add2<Vector2>("Vector2", x => { var v = (Vector2)x; return $"({v.X}, {v.Y})"; });
    }

    public void RenderNode(string name, object target)
    {
        var meta = new ReflectorMetadata(name, null, null, null, null, null);
        var state = new ReflectorState(target, null, -1, meta);
        var reflector = GetReflectorForInstance(state.Target);
        reflector(state);
    }

    public Reflector GetReflectorForInstance(object target)
        => target == null
            ? _nullReflector
            : GetReflectorForType(target.GetType());

    Reflector GetReflectorForType(Type type)
    {
        if (_reflectors.TryGetValue(type, out var reflector))
            return reflector;

        reflector = BuildReflector(type);
        _reflectors[type] = reflector;
        return reflector;
    }

    Reflector BuildReflector(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (typeof(Enum).IsAssignableFrom(type))
            return EnumReflector.Build(type);

        if (typeof(IEnumerable).IsAssignableFrom(type))
            return new EnumerableReflector(this, type).Reflect;

        if (typeof(Component).IsAssignableFrom(type))
            return new ObjectReflector(this, type).ReflectComponent;

        return new ObjectReflector(this, type).Reflect;
    }

    public void RenderOptions()
    {
        ImGui.Checkbox("Edit Mode", ref _editMode);
        ImGui.SameLine();

        if (ImGui.Button("Save"))
        {
        }

        if (EditTarget != null)
            RenderEditPopup();
    }

    void RenderEditPopup()
    {
        bool open = true;
        if (ImGui.Begin("Edit Member", ref open))
        {
            ImGui.Text($"Editing {EditTarget.ParentType}.{EditTarget.Name}");

            if (EditTarget.Options != null && ImGui.Button("Reset to Defaults"))
                EditTarget.Options = null;

            if (EditTarget.Options == null && ImGui.Button("Override Defaults"))
                EditTarget.Options = new DiagEditAttribute { Style = DiagEditStyle.Label };

            var options = EditTarget.Options;
            if (options != null)
            {
                int style = (int)options.Style;
                if (ImGui.Combo("Style", ref style, StyleNames, StyleNames.Length))
                    options.Style = Styles[style];

                EditValue("Min", static x => x.Options.Min, static (x, v) => x.Options.Min = v);
                EditValue("Max", static x => x.Options.Max, static (x, v) => x.Options.Max = v);

                // EditTarget.Options.Min MinProperty
                // EditTarget.Options.Max MaxProperty
                // EditTarget.Options.MaxLength
            }

            ImGui.End();
        }

        if (!open)
            EditTarget = null;
    }

    void EditValue(string label, Func<ReflectorMetadata, object> getter, Action<ReflectorMetadata, object> setter)
    {
        if (EditTarget.ValueType == typeof(int))
        {
            int currentVal = getter(EditTarget) is int intValue ? intValue : int.MinValue;
            if (ImGui.InputInt(label, ref currentVal))
                setter(EditTarget, currentVal == int.MinValue ? null : currentVal);

            if (currentVal != int.MinValue)
            {
                ImGui.SameLine();
                if (ImGui.Button("Clear##Clear" + label))
                    setter(EditTarget, null);
            }
        }
        else if (EditTarget.ValueType == typeof(float))
        {
            float currentVal = getter(EditTarget) is float intValue ? intValue : float.NaN;
            if (ImGui.InputFloat(label, ref currentVal))
                setter(EditTarget, float.IsNaN(currentVal) ? null : currentVal);

            if (!float.IsNaN(currentVal))
            {
                ImGui.SameLine();
                if (ImGui.Button("Clear##Clear" + label))
                    setter(EditTarget, null);
            }
        }
    }
}