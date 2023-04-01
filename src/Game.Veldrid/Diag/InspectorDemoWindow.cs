using System;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Reflection;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion.Game.Veldrid.Diag;

public class InspectorDemoWindow : Component, IImGuiWindow
{
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public enum SomeEnum
    {
        First,
        Second,
        Third
    }

    [Flags]
    public enum SomeFlags
    {
        One = 1,
        Two = 2,
        Four = 4,
        Eight = 8
    }
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix

    class TestObject
    {
        [DiagEdit(Style = DiagEditStyle.Checkboxes)] public bool BoolProp { get; set; }
        [DiagEdit(Style = DiagEditStyle.NumericSlider, Min = 0, Max = 100)] public int[] IntArray = new int[16];
        [DiagEdit(Style = DiagEditStyle.NumericInput)] public int IntField;
        [DiagEdit(Style = DiagEditStyle.NumericSlider, Min = 0, Max = 100)] public int IntProp { get; set; }
        [DiagEdit(Style = DiagEditStyle.NumericSlider, Min = -32, Max = 32)] public int? NullableIntProp { get; set; }
        [DiagEdit(Style = DiagEditStyle.NumericSlider, Min = -1, Max = 1)] public float FloatProp { get; set; }
        [DiagEdit(Style = DiagEditStyle.ColorPicker)] public Vector3 Vec3 { get; set; } = new(0.3f, 0.5f, 0.8f);
        [DiagEdit(Style = DiagEditStyle.ColorPicker)] public Vector4 Color { get; set; } = new(0, 0.5f, 1.0f, 1.0f);
        [DiagEdit(Style=DiagEditStyle.Dropdown)] public SomeEnum SimpleEnum { get; set; } = SomeEnum.Second;
        [DiagEdit(Style = DiagEditStyle.Checkboxes)] public SomeFlags FlagsEnum { get; set; } = SomeFlags.Two | SomeFlags.Four;
        [DiagEdit(Style = DiagEditStyle.Text, MaxLength = 32)] public string Text { get; set; } = "Foo";
    }

    readonly TestObject _testObject = new();
    readonly string _name;

    public InspectorDemoWindow(int id)
    {
        _name = $"Inspector Demo###IDemo{id}";
    }

    public void Draw(GraphicsDevice device)
    {
        ReflectorUtil.SwapAuxiliaryState();
        var state = TryResolve<IGameState>();
        if (state == null)
            return;

        ImGui.Begin(_name);
        RenderNode("Test", _testObject);
        ImGui.End();
    }

    static void RenderNode(string name, object target)
    {
        var meta = new ReflectorMetadata(name, null, null, null);
        var state = new ReflectorState(target, null, -1, meta);
        var reflector = ReflectorManager.Instance.GetReflectorForInstance(state.Target);
        reflector(state);
    }
}