using System.Collections.Generic;
using System.Numerics;
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
                {

                }
            }),
            new Handler<MouseLookInputMode, InputEvent>((x,e) => x.OnInput(e)), 
        };

        void OnInput(InputEvent e)
        {
            if (!_isActive)
                return;

            if(e.MouseDelta != Vector2.Zero)
                Raise(new CameraRotateEvent(e.MouseDelta.X * -0.01f, e.MouseDelta.Y * -0.01f));
        }

        bool _isActive;

        public MouseLookInputMode() : base(Handlers) { }
    }
}