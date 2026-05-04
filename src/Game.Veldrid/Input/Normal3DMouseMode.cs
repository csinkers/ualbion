using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Base;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Input;
using Veldrid.Sdl2;

namespace UAlbion.Game.Veldrid.Input;

public class Normal3DMouseMode : Component
{
    /*
    Cursor regions:

    0: No action
    +: Move forward
    -: Move back
    ^: Look up
    v: Look down
    <: Strafe left
    >: Strafe right

    a: Turn 90° left
    b: Turn 90° right
    c: Turn 180° left
    d: Turn 180° right

    1: Turn left
    2: Turn right
    3: Move back and turn left
    4: Move back and turn right
    5: Move forward and turn left
    6: Move forward and turn right

    |0|   1    |     2    |    3   |4|
    |--------------------------------|-
    |a5555555555^^^^^^^^^^6666666666b|0
    |11111111111++++++++++22222222222|-
    |11111111111++++++++++22222222222|1
    |11111111111000000000022222222222|-
    |<<<<<<<<<<<0000000000>>>>>>>>>>>|2
    |<<<<<<<<<<<0000000000>>>>>>>>>>>|-
    |33333333333----------44444444444|3
    |33333333333----------44444444444|-
    |c3333333333vvvvvvvvvv4444444444d|4
    |--------------------------------|-
    */

    enum Zone
    {
        None,
        Forward,
        Back,
        TurnLeft,
        TurnRight,
        StrafeLeft,
        StrafeRight,
        ForwardLeft,
        ForwardRight,
        BackLeft,
        BackRight,
        Left90,
        Right90,
        Left180,
        Right180,
        LookUp,
        LookDown,
        StatusBar
    }

    const int ZoneMapStride = 5;
    static readonly int[] HGuides = [ 1, 100, 280, 359 ];
    static readonly int[] VGuides = [ 1,  64, 128, 191, 192 ];
    static readonly Zone[] Map =
    [
        Zone.Left90,     Zone.ForwardLeft, Zone.LookUp,    Zone.ForwardRight, Zone.Right90,
        Zone.TurnLeft,   Zone.TurnLeft,    Zone.Forward,   Zone.TurnRight,    Zone.TurnRight,
        Zone.StrafeLeft, Zone.StrafeLeft,  Zone.None,      Zone.StrafeRight,  Zone.StrafeRight,
        Zone.BackLeft,   Zone.BackLeft,    Zone.Back,      Zone.BackRight,    Zone.BackRight,
        Zone.Left180,    Zone.BackLeft,    Zone.LookDown,  Zone.BackRight,    Zone.Right180,
        Zone.Left180,    Zone.StatusBar,   Zone.StatusBar, Zone.StatusBar,    Zone.Right180,
    ];

    static readonly Zone[] PressedMap = // Map to use while dragging with left button held
    [
        Zone.TurnLeft,   Zone.ForwardLeft, Zone.Forward,   Zone.ForwardRight, Zone.TurnRight,
        Zone.TurnLeft,   Zone.TurnLeft,    Zone.Forward,   Zone.TurnRight,    Zone.TurnRight,
        Zone.StrafeLeft, Zone.StrafeLeft,  Zone.None,      Zone.StrafeRight,  Zone.StrafeRight,
        Zone.BackLeft,   Zone.BackLeft,    Zone.Back,      Zone.BackRight,    Zone.BackRight,
        Zone.BackLeft,   Zone.BackLeft,    Zone.Back,      Zone.BackRight,    Zone.BackRight,
        Zone.BackLeft,   Zone.StatusBar,   Zone.StatusBar, Zone.StatusBar,    Zone.BackRight,
    ];

    readonly PartyMove3DEvent _moveEvent = new();
    readonly UiLeftClickEvent _uiLeftClickEvent = new();
    readonly UiLeftReleaseEvent _uiLeftReleaseEvent = new();
    readonly UiRightClickEvent _uiRightClickEvent = new();
    readonly UiRightReleaseEvent _rightReleaseEvent = new();
    readonly UiScrollEvent _uiScrollEvent = new(0);
    readonly List<Selection> _hits = [];

    public Normal3DMouseMode()
    {
        On<MouseInputEvent>(OnInput);
        On<EngineUpdateEvent>(_ =>
        {
            for (int i = 0; i < HGuides.Length; i++)
            {
                DebugUi.Line2D(CoordinateSystem.Ui2D,
                    new Vector2(HGuides[i], UiConstants.UiExtents.Top),
                    new Vector2(HGuides[i], UiConstants.UiExtents.Bottom));
            }

            for (int i = 0; i < VGuides.Length; i++)
            {
                DebugUi.Line2D(CoordinateSystem.Ui2D,
                    new Vector2(UiConstants.UiExtents.Left, VGuides[i]),
                    new Vector2(UiConstants.UiExtents.Right, VGuides[i]));
            }
        });
    }

    protected override void Subscribed()
    {
        Raise(new SetCursorEvent(CoreGfx.CursorSelected));
    }

    void OnInput(MouseInputEvent e)
    {
        var window = Resolve<IGameWindow>();
        var normPosition = window.PixelToNorm(e.MousePosition);
        Vector2 uiPosition = window.NormToUi(normPosition);
        uiPosition.X = Math.Clamp(uiPosition.X, 0, UiConstants.UiExtents.Width - 1);
        uiPosition.Y = Math.Clamp(uiPosition.Y, 0, UiConstants.UiExtents.Height - 1);

        bool justPressed = e.CheckMouse(MouseButton.Left, true);
        var zoneIndex = GetZoneIndexForUiPosition(uiPosition);
        var zone = GetZoneByIndex(zoneIndex, justPressed);
        var rect = GetRectForZoneIndex(zoneIndex);

        float dx = (uiPosition.X - rect.X) / rect.Width;
        float dy = (uiPosition.Y - rect.Y) / rect.Height;

        if (uiPosition.X < UiConstants.UiExtents.Width / 2.0f)
            dx = 1.0f - dx;

        if (uiPosition.Y < UiConstants.UiExtents.Height / 2.0f)
            dy = 1.0f - dy;

        var intensity = new Vector2(dx, dy);
        var cursor = GetCursorForZone(zone);

        if (ImGui.Begin("MouseMode"))
        {
            ImGui.Text($"UI: {uiPosition}");
            ImGui.Text($"ZoneIndex: {zoneIndex} ({zoneIndex%ZoneMapStride}, {zoneIndex/ZoneMapStride})");
            ImGui.Text($"Zone: {zone} [{rect}]");
            ImGui.Text($"Intensity: {intensity}");
            ImGui.End();
        }

        if (Resolve<ICursorManager>().CursorId != cursor)
            Raise(new SetCursorEvent(cursor));

        // var windowState = Resolve<IWindowManager>();
        // var delta = e.MousePosition - new Vector2((int)(windowState.PixelWidth / 2), (int)(windowState.PixelHeight / 2));
        _hits.Clear();
        Resolve<ISelectionManager>()?.CastRayFromScreenSpace(_hits, e.MousePosition, false, true);

        _moveEvent.Velocity = Vector2.Zero;
        _moveEvent.Yaw = 0;
        _moveEvent.Pitch = 0;

        if (e.IsMouseDown(MouseButton.Left))
            HandleZone(zone, intensity, justPressed);

        // Clicks are targeted, releases are broadcast. e.g. if you click and drag a slider and move outside
        // its hover area, then it should switch to "ClickedBlurred". If you then release the button while
        // still outside its hover area and releases were broadcast, it would never receive the release and
        // it wouldn't be able to transition back to Normal
        if (_hits.Count > 0)
        {
            if (e.CheckMouse(MouseButton.Right, true))
                Distribute(_uiRightClickEvent, _hits, x => x.Target as IComponent);

            if (e.CheckMouse(MouseButton.Left, true))
                Distribute(_uiLeftClickEvent, _hits, x => x.Target as IComponent);

            if ((int)e.WheelDelta.Y != 0)
            {
                _uiScrollEvent.Delta = (int)e.WheelDelta.Y;
                Distribute(_uiScrollEvent, _hits, x => x.Target as IComponent);
            }
        }

        if (e.CheckMouse(MouseButton.Left, false))
            Raise(_uiLeftReleaseEvent);
    }

    static Rectangle GetRectForZoneIndex(int zoneIndex)
    {
        if (zoneIndex < 0 || zoneIndex >= Map.Length)
            return new Rectangle(0, 0, 0, 0);

        int xIndex = zoneIndex % ZoneMapStride;
        int yIndex = zoneIndex / ZoneMapStride;

        int x = xIndex == 0 ? 0 : HGuides[xIndex - 1];
        int y = yIndex == 0 ? 0 : VGuides[yIndex - 1];
        int width  = (xIndex < HGuides.Length ? HGuides[xIndex] : UiConstants.UiExtents.Right)  - x;
        int height = (yIndex < VGuides.Length ? VGuides[yIndex] : UiConstants.UiExtents.Bottom) - y;

        return new Rectangle(x, y, width, height);
    }

    void HandleZone(Zone zone, Vector2 intensity, bool justPressed)
    {
        switch (zone)
        {
            case Zone.None:
                break;

            case Zone.Forward:
                _moveEvent.Velocity = new Vector2(0, intensity.Y);
                Raise(_moveEvent);
                break;

            case Zone.Back:
                _moveEvent.Velocity = new Vector2(0, -intensity.Y);
                Raise(_moveEvent);
                break;

            case Zone.TurnLeft:
                _moveEvent.Yaw = -intensity.X;
                Raise(_moveEvent);
                break;

            case Zone.TurnRight:
                _moveEvent.Yaw = intensity.X;
                Raise(_moveEvent);
                break;

            case Zone.StrafeLeft:
                _moveEvent.Velocity = new Vector2(-intensity.X, 0);
                Raise(_moveEvent);
                break;

            case Zone.StrafeRight:
                _moveEvent.Velocity = new Vector2(intensity.X, 0);
                Raise(_moveEvent);
                break;

            case Zone.ForwardLeft:
                _moveEvent.Velocity = new Vector2(0, intensity.Y);
                _moveEvent.Yaw = -intensity.X;
                Raise(_moveEvent);
                break;

            case Zone.ForwardRight:
                _moveEvent.Velocity = new Vector2(0, intensity.Y);
                _moveEvent.Yaw = intensity.X;
                Raise(_moveEvent);
                break;

            case Zone.BackLeft:
                _moveEvent.Velocity = new Vector2(0, -intensity.Y);
                _moveEvent.Yaw = -intensity.X;
                Raise(_moveEvent);
                break;

            case Zone.BackRight:
                _moveEvent.Velocity = new Vector2(0, -intensity.Y);
                _moveEvent.Yaw = intensity.X;
                Raise(_moveEvent);
                break;

            case Zone.Left90:
                if (justPressed)
                    Raise(new PartyTurn3DEvent(-90));
                break;
            case Zone.Right90:
                if (justPressed)
                    Raise(new PartyTurn3DEvent(90));
                break;
            case Zone.Left180:
                if (justPressed)
                    Raise(new PartyTurn3DEvent(-180));
                break;
            case Zone.Right180:
                if (justPressed)
                    Raise(new PartyTurn3DEvent(180));
                break;

            case Zone.LookUp:
                _moveEvent.Pitch = 1;
                Raise(_moveEvent);
                break;

            case Zone.LookDown:
                _moveEvent.Pitch = -1;
                Raise(_moveEvent);
                break;

            case Zone.StatusBar:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(zone));
        }
    }

    static int GetZoneIndexForUiPosition(Vector2 uiPosition)
    {
        int xIndex = 0;
        if (uiPosition.X < 0)
            return -1;

        for (int i = 0; i < HGuides.Length; i++)
        {
            if (!(uiPosition.X >= HGuides[i]))
                continue;

            xIndex = i + 1;
        }

        int yIndex = 0;
        for (int i = 0; i < VGuides.Length; i++)
        {
            if (!(uiPosition.Y >= VGuides[i]))
                continue;

            yIndex = i + 1;
        }

        return xIndex + yIndex * ZoneMapStride;
    }

    static Zone GetZoneByIndex(int index, bool justPressed)
    {
        var map = justPressed ? Map : PressedMap;
        return index >= 0 && index < map.Length
            ? map[index]
            : Zone.None;
    }

    static SpriteId GetCursorForZone(Zone zone) =>
        zone switch
        {
            Zone.None => CoreGfx.CursorSelected,
            Zone.Forward => CoreGfx.Cursor3dUp,
            Zone.Back => CoreGfx.Cursor3dDown,
            Zone.TurnLeft => CoreGfx.Cursor3dTurnLeft90,
            Zone.TurnRight => CoreGfx.Cursor3dTurnRight90,
            Zone.StrafeLeft => CoreGfx.Cursor3dLeft,
            Zone.StrafeRight => CoreGfx.Cursor3dRight,
            Zone.ForwardLeft => CoreGfx.CursorUpLeft,
            Zone.ForwardRight => CoreGfx.CursorUpRight,
            Zone.BackLeft => CoreGfx.Cursor3dTurnLeft180,
            Zone.BackRight => CoreGfx.Cursor3dTurnRight180,
            Zone.Left90 => CoreGfx.ArrowTurnLeft90,
            Zone.Right90 => CoreGfx.ArrowTurnRight90,
            Zone.Left180 => CoreGfx.ArrowTurnLeft180,
            Zone.Right180 => CoreGfx.ArrowTurnRight180,
            Zone.LookUp => CoreGfx.ArrowLookUp,
            Zone.LookDown => CoreGfx.ArrowLookDown,
            Zone.StatusBar => CoreGfx.Cursor,
            _ => CoreGfx.Cursor
        };
}
