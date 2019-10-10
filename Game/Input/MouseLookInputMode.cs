using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
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

    public class MouseLookInputMode : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<MouseLookInputMode, SetInputModeEvent>((x,e) =>
            {
                var activating = e.Mode == InputMode.MouseLook && !x._isActive;
                var deactivating = e.Mode != InputMode.MouseLook && x._isActive;
                if (activating)
                {
                    x._isActive = true;
                    x.Raise(new SetCursorEvent(CoreSpriteId.CursorCrossUnselected));
                }

                if (deactivating)
                    x._isActive = false;
            }),
            new Handler<MouseLookInputMode, InputEvent>((x,e) => x.OnInput(e)), 
            new Handler<MouseLookInputMode, PostUpdateEvent>((x, e) =>
            {
                if (!x._isActive) return;
                var windowState = x.Exchange.Resolve<IWindowManager>();
                x.Raise(new SetCursorPositionEvent(windowState.PixelWidth / 2, windowState.PixelHeight / 2));
            }),
        };

        void OnInput(InputEvent e)
        {
            if (!_isActive /*|| ImGui.GetIO().WantCaptureMouse*/)
                return;

            var windowState = Exchange.Resolve<IWindowManager>();
            var delta = e.Snapshot.MousePosition - new Vector2((float)windowState.PixelWidth / 2, (float)windowState.PixelHeight / 2);

            if (delta.LengthSquared() > float.Epsilon)
                Raise(new CameraRotateEvent(delta.X * -0.003f, delta.Y * -0.003f));
        }

        bool _isActive;

        public MouseLookInputMode() : base(Handlers) { }
    }
}