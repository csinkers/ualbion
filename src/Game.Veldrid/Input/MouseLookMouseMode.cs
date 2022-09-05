using System.Collections.Generic;
using Veldrid;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Input;
/*
if (!ImGui.GetIO().WantCaptureMouse
    && (InputTracker.GetMouseButton(MouseButton.Left) || InputTracker.GetMouseButton(MouseButton.Right)))
{
    if (!_mousePressed)
    {
        _mousePressed = true;
        _mousePressedPos = InputTracker.MousePosition;
        Sdl2Native.SDL_ShowCursor(0);
        // Sdl2Native.SDL_SetWindowGrab(_window.SdlWindowHandle, true);
    }
    Vector2 mouseDelta = _mousePressedPos - InputTracker.MousePosition;
    Sdl2Native.SDL_WarpMouseInWindow(_window.SdlWindowHandle, (int)_mousePressedPos.X, (int)_mousePressedPos.Y);
    Yaw += mouseDelta.X * 0.002f;
    Pitch += mouseDelta.Y * 0.002f;
}
else if(_mousePressed)
{
    Sdl2Native.SDL_WarpMouseInWindow(_window.SdlWindowHandle, (int)_mousePressedPos.X, (int)_mousePressedPos.Y);
    // Sdl2Native.SDL_SetWindowGrab(_window.SdlWindowHandle, false);
    Sdl2Native.SDL_ShowCursor(1);
    _mousePressed = false;
}

UpdateViewMatrix();
*/

public class MouseLookMouseMode : Component
{
    readonly CameraRotateEvent _cameraRotateEvent = new(0,0);
    readonly ConfineMouseToWindowEvent _confineMouseToWindowEvent = new(true);
    readonly SetCursorEvent _setCursorEvent = new(Base.CoreGfx.CursorCrossUnselected);
    readonly SetRelativeMouseModeEvent _setRelativeMouseModeEvent = new(true);
    readonly UiLeftClickEvent _uiLeftClickEvent = new();
    readonly UiLeftReleaseEvent _uiLeftReleaseEvent = new();
    readonly UiRightClickEvent _uiRightClickEvent = new();
    readonly UiScrollEvent _uiScrollEvent = new(0);
    readonly List<Selection> _hits = new();

    bool _firstEvent;

    public MouseLookMouseMode()
    {
        On<InputEvent>(OnInput);
        On<FocusGainedEvent>(_ => AcquireMouse());
        On<FocusLostEvent>(_ => ReleaseMouse());
        On<PostGameUpdateEvent>(_ =>
        {
            //var windowState = Resolve<IWindowManager>();
            //Raise(new SetCursorPositionEvent(windowState.PixelWidth / 2, windowState.PixelHeight / 2));
        });
    }

    protected override void Subscribed()
    {
        Raise(_setCursorEvent);
        AcquireMouse();
        _firstEvent = true;
    }

    protected override void Unsubscribed() => ReleaseMouse();

    void AcquireMouse()
    {
        _setRelativeMouseModeEvent.Enabled = true;
        _confineMouseToWindowEvent.Enabled = true;
        Raise(_setRelativeMouseModeEvent);
        Raise(_confineMouseToWindowEvent);
    }

    void ReleaseMouse()
    {
        _setRelativeMouseModeEvent.Enabled = false;
        _confineMouseToWindowEvent.Enabled = false;
        Raise(_setRelativeMouseModeEvent);
        Raise(_confineMouseToWindowEvent);
    }

    void OnInput(InputEvent e)
    {
        if (_firstEvent) // Ignore the first event to prevent the view jumping about due to the prior cursor position
        {
            _firstEvent = false;
            return;
        }

        // var windowState = Resolve<IWindowManager>();
        // var delta = e.Snapshot.MousePosition - new Vector2((int)(windowState.PixelWidth / 2), (int)(windowState.PixelHeight / 2));
        _hits.Clear();
        Resolve<ISelectionManager>()?.CastRayFromScreenSpace(_hits, e.Snapshot.MousePosition, false, true);

        if (e.MouseDelta.LengthSquared() > float.Epsilon)
        {
            var sensitivity = GetVar(GameVars.Ui.MouseLookSensitivity) / -1000;
            _cameraRotateEvent.Yaw = e.MouseDelta.X * sensitivity;
            _cameraRotateEvent.Pitch = e.MouseDelta.Y * sensitivity;
            Raise(_cameraRotateEvent);
        }

        // Clicks are targeted, releases are broadcast. e.g. if you click and drag a slider and move outside
        // its hover area, then it should switch to "ClickedBlurred". If you then release the button while
        // still outside its hover area and releases were broadcast, it would never receive the release and
        // it wouldn't be able to transition back to Normal
        if (_hits.Count > 0)
        {
            if (e.Snapshot.CheckMouse(MouseButton.Right, true))
                Distribute(_uiRightClickEvent, _hits, x => x.Target as IComponent);

            if (e.Snapshot.CheckMouse(MouseButton.Left, true))
                Distribute(_uiLeftClickEvent, _hits, x => x.Target as IComponent);

            if ((int)e.Snapshot.WheelDelta != 0)
            {
                _uiScrollEvent.Delta = (int)e.Snapshot.WheelDelta;
                Distribute(_uiScrollEvent, _hits, x => x.Target as IComponent);
            }
        }

        if (e.Snapshot.CheckMouse(MouseButton.Left, false))
            Raise(_uiLeftReleaseEvent);
    }
}