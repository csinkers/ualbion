using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace UAlbion.Core.Veldrid
{
    class WindowHolder : Component, IDisposable
    {
        readonly WindowManager _windowManager;
        Sdl2Window _window;
        DateTime _lastTitleUpdateTime;
        Vector2? _pendingCursorUpdate;
        public Sdl2Window Window => _window;
        public string WindowTitle { get; set; }

        public WindowHolder()
        {
            On<SetCursorPositionEvent>(e => _pendingCursorUpdate = new Vector2(e.X, e.Y));
            On<ToggleFullscreenEvent>(e => ToggleFullscreenState());
            On<ToggleHardwareCursorEvent>(e => { if (_window != null) _window.CursorVisible = !_window.CursorVisible; });
            On<ToggleResizableEvent>(e => { if (_window != null) _window.Resizable = !_window.Resizable; });
            On<ToggleVisibleBorderEvent>(e => { if (_window != null) _window.BorderVisible = !_window.BorderVisible; });
            On<ConfineMouseToWindowEvent>(e => { if (_window != null) Sdl2Native.SDL_SetWindowGrab(_window.SdlWindowHandle, e.Enabled); });
            On<SetRelativeMouseModeEvent>(e =>
            {
                if (_window == null) return;
                Sdl2Native.SDL_SetRelativeMouseMode(e.Enabled);
                if (!e.Enabled)
                    Sdl2Native.SDL_WarpMouseInWindow(_window.SdlWindowHandle, _window.Width / 2, _window.Height / 2);
            });
            _windowManager = AttachChild(new WindowManager());
        }

        public void CreateWindow(int x, int y, int width, int height)
        {
            if (_window != null)
                return;

            var windowInfo = new WindowCreateInfo
            {
                X = x,
                Y = y,
                WindowWidth = _window?.Width ?? width,
                WindowHeight = _window?.Height ?? height,
                WindowInitialState = _window?.WindowState ?? WindowState.Normal,
                WindowTitle = "UAlbion"
            };

            _window = VeldridStartup.CreateWindow(ref windowInfo);
            _window.CursorVisible = false;
            _window.Resized += () => Raise(new WindowResizedEvent(_window.Width, _window.Height));
            _window.Closed += () => Raise(new WindowClosedEvent());
            _window.FocusGained += () => Raise(new FocusGainedEvent());
            _window.FocusLost += () => Raise(new FocusLostEvent());
            Raise(new WindowResizedEvent(_window.Width, _window.Height));
        }

        void ToggleFullscreenState()
        {
            if (_window == null)
                return;
            bool isFullscreen = _window.WindowState == WindowState.BorderlessFullScreen;
            _window.WindowState = isFullscreen ? WindowState.Normal : WindowState.BorderlessFullScreen;
        }

        void SetTitle()
        {
            if (DateTime.UtcNow - _lastTitleUpdateTime <= TimeSpan.FromSeconds(1)) 
                return;

            var engine = Resolve<IEngine>();
            _window.Title = $"{WindowTitle} - {engine.FrameTimeText}";
            _lastTitleUpdateTime = DateTime.UtcNow;
        }

        public void Dispose() => _window.Close();
        public void PumpEvents(double deltaSeconds)
        {
            SetTitle();
            Sdl2Events.ProcessEvents();
            var snapshot = _window.PumpEvents();
            if (_window != null)
            {
                if (_pendingCursorUpdate.HasValue && _window.Focused)
                {
                    using (PerfTracker.FrameEvent("3 Warping mouse"))
                    {
                        Sdl2Native.SDL_WarpMouseInWindow(
                            _window.SdlWindowHandle,
                            (int)_pendingCursorUpdate.Value.X,
                            (int)_pendingCursorUpdate.Value.Y);

                        _pendingCursorUpdate = null;
                    }
                }

                using (PerfTracker.FrameEvent("4 Raising input event"))
                    Raise(new InputEvent(deltaSeconds, snapshot, _window.MouseDelta));
            }
        }
    }
}