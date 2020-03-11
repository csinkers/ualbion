using System.Numerics;
using SerdesNet;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Input
{
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
        const float Sensitivity = 2;
        static readonly HandlerSet Handlers = new HandlerSet(
            H<MouseLookMouseMode, InputEvent>((x,e) => x.OnInput(e)),
            H<MouseLookMouseMode, PostUpdateEvent>((x, e) =>
            {
                var windowState = x.Resolve<IWindowManager>();
                x.Raise(new SetCursorPositionEvent(windowState.PixelWidth / 2, windowState.PixelHeight / 2));
            })
        );

        public override void Subscribed()
        {
            Raise(new SetCursorEvent(CoreSpriteId.CursorCrossUnselected));
            base.Subscribed();
        }

        void OnInput(InputEvent e)
        {
            var windowState = Resolve<IWindowManager>();
            var delta = e.Snapshot.MousePosition - new Vector2((int)(windowState.PixelWidth / 2), (int)(windowState.PixelHeight / 2));

            if (delta.LengthSquared() > float.Epsilon)
                Raise(new CameraRotateEvent(delta.X * (Sensitivity / -1000), delta.Y * (Sensitivity / -1000)));
        }

        public MouseLookMouseMode() : base(Handlers) { }
    }
}
