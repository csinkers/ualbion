using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using Veldrid;

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
        bool _firstEvent;

        public MouseLookMouseMode()
        {
            On<InputEvent>(OnInput);
            On<FocusGainedEvent>(_ => AcquireMouse());
            On<FocusLostEvent>(_ => ReleaseMouse());
            On<PostUpdateEvent>(e =>
            {
                //var windowState = Resolve<IWindowManager>();
                //Raise(new SetCursorPositionEvent(windowState.PixelWidth / 2, windowState.PixelHeight / 2));
            });
        }

        protected override void Subscribed()
        {
            Raise(new SetCursorEvent(Base.CoreSprite.CursorCrossUnselected));
            AcquireMouse();
            _firstEvent = true;
        }

        protected override void Unsubscribed() => ReleaseMouse();

        void AcquireMouse()
        {
            Raise(new SetRelativeMouseModeEvent(true));
            Raise(new ConfineMouseToWindowEvent(true));
        }

        void ReleaseMouse()
        {
            Raise(new ConfineMouseToWindowEvent(false));
            Raise(new SetRelativeMouseModeEvent(false));
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
            var hits = Resolve<ISelectionManager>()?.CastRayFromScreenSpace(e.Snapshot.MousePosition, true);

            if (e.MouseDelta.LengthSquared() > float.Epsilon)
            {
                var config = Resolve<GameConfig>();
                var sensitivity = config.UI.MouseLookSensitivity / -1000;
                Raise(new CameraRotateEvent(e.MouseDelta.X * sensitivity, e.MouseDelta.Y * sensitivity));
            }

            // Clicks are targeted, releases are broadcast. e.g. if you click and drag a slider and move outside
            // its hover area, then it should switch to "ClickedBlurred". If you then release the button while
            // still outside its hover area and releases were broadcast, it would never receive the release and
            // it wouldn't be able to transition back to Normal
            if (hits != null)
            {
                if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && x.Down))
                    Distribute(new UiRightClickEvent(), hits);

                if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && x.Down))
                    Distribute(new UiLeftClickEvent(), hits);

                if ((int)e.Snapshot.WheelDelta != 0)
                    Distribute(new UiScrollEvent((int)e.Snapshot.WheelDelta), hits);
            }

            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && !x.Down))
                Raise(new UiLeftReleaseEvent());
        }

        void Distribute(ICancellableEvent e, IList<Selection> hits)
        {
            foreach (var hit in hits)
            {
                if (!e.Propagating) break;
                var component = hit.Target as IComponent;
                component?.Receive(e, this);
            }
        }
    }
}
