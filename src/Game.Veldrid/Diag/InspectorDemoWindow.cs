using System;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Diag;
using UAlbion.Core.Veldrid.Reflection;
using UAlbion.Game.State;

namespace UAlbion.Game.Veldrid.Diag;

public class InspectorDemoWindow(string name) : Component, IImGuiWindow
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

#pragma warning disable CS0414 // Field is assigned but its value is never used
    // ReSharper disable UnusedMember.Local
    sealed class TestObject
    {
        [DiagEdit(Style = DiagEditStyle.Checkboxes)] public bool BoolProp { get; set; }
        [DiagEdit(Style = DiagEditStyle.NumericSlider, Min = 0, Max = 100)] public int[] IntArray = new int[16];
        [DiagEdit(Style = DiagEditStyle.NumericInput)] public int IntField = 5;
        [DiagEdit(Style = DiagEditStyle.NumericSlider, Min = 0, Max = 100)] public int IntProp { get; set; }
        [DiagEdit(Style = DiagEditStyle.NumericSlider, Min = -32, Max = 32)] public int? NullableIntProp { get; set; }
        [DiagEdit(Style = DiagEditStyle.NumericSlider, Min = -1, Max = 1)] public float FloatProp { get; set; }
        [DiagEdit(Style = DiagEditStyle.ColorPicker)] public Vector3 Vec3 { get; set; } = new(0.3f, 0.5f, 0.8f);
        [DiagEdit(Style = DiagEditStyle.ColorPicker)] public Vector4 Color { get; set; } = new(0, 0.5f, 1.0f, 1.0f);
        [DiagEdit(Style = DiagEditStyle.Dropdown)] public SomeEnum SimpleEnum { get; set; } = SomeEnum.Second;
        [DiagEdit(Style = DiagEditStyle.Checkboxes)] public SomeFlags FlagsEnum { get; set; } = SomeFlags.Two | SomeFlags.Four;
        [DiagEdit(Style = DiagEditStyle.Text, MaxLength = 32)] public string Text { get; set; } = "Foo";
    }
    // ReSharper restore UnusedMember.Local
#pragma warning restore CS0414 // Field is assigned but its value is never used

    readonly TestObject _testObject = new();
    public string Name { get; } = name;

    public ImGuiWindowDrawResult Draw()
    {
        ReflectorUtil.SwapAuxiliaryState();
        var state = TryResolve<IGameState>();
        if (state == null)
            return ImGuiWindowDrawResult.Closed;

        bool open = true;
        ImGui.Begin(Name, ref open);

        var reflectorManager = Resolve<ReflectorManager>();
        reflectorManager.RenderNode("Test", _testObject);

        ImGui.End();
        return open ? ImGuiWindowDrawResult.None : ImGuiWindowDrawResult.Closed;
    }
}