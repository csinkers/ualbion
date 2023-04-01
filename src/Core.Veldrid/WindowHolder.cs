using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace UAlbion.Core.Veldrid;

class WindowHolder : Component, IDisposable
{
    readonly PreviewInputEvent _previewEvent = new();
    readonly KeyboardInputEvent _keyboardEvent = new();
    readonly MouseInputEvent _mouseEvent = new();
    Sdl2Window _window;
    DateTime _lastTitleUpdateTime;
    Vector2? _pendingCursorUpdate;
    WindowState _lastState;
    public Sdl2Window Window => _window;
    public string WindowTitle { get; set; }

    public WindowHolder()
    {
        On<SetCursorPositionEvent>(e => _pendingCursorUpdate = new Vector2(e.X, e.Y));
        On<ToggleFullscreenEvent>(_ => ToggleFullscreenState());
        On<ShowHardwareCursorEvent>(e => { if (_window != null) _window.CursorVisible = e.Show; });
        On<ToggleResizableEvent>(_ => { if (_window != null) _window.Resizable = !_window.Resizable; });
        On<ToggleVisibleBorderEvent>(_ => { if (_window != null) _window.BorderVisible = !_window.BorderVisible; });
        On<ConfineMouseToWindowEvent>(e => { if (_window != null) Sdl2Native.SDL_SetWindowGrab(_window.SdlWindowHandle, e.Enabled); });
        On<SetRelativeMouseModeEvent>(e =>
        {
            if (_window == null) return;
            Sdl2Native.SDL_SetRelativeMouseMode(e.Enabled);
            if (!e.Enabled)
                Sdl2Native.SDL_WarpMouseInWindow(_window.SdlWindowHandle, _window.Width / 2, _window.Height / 2);
        });
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
        _window.Resized += () =>
        {
            if (_lastState != _window.WindowState)
            {
                if (_window.WindowState == WindowState.Minimized)
                    Raise(new WindowHiddenEvent());
                else
                    Raise(new WindowShownEvent());

                _lastState = _window.WindowState;
            }

            Raise(new WindowResizedEvent(_window.Width, _window.Height));
        };
        _window.Closed += () => Raise(new WindowClosedEvent());
        _window.FocusGained += () => Raise(new FocusGainedEvent());
        _window.FocusLost += () => Raise(new FocusLostEvent());
        _lastState = _window.WindowState;
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

    public void Dispose() => _window?.Close();
    public void PumpEvents(double deltaSeconds)
    {
        SetTitle();
        using (PerfTracker.FrameEvent("SDL events"))
            Sdl2Events.ProcessEvents();

        var snapshot = _window.PumpEvents();

        if (_window == null) // Check if the window was closed while pumping events
            return;

        if (_pendingCursorUpdate.HasValue && _window.Focused)
        {
            using (PerfTracker.FrameEvent("Warping mouse"))
            {
                Sdl2Native.SDL_WarpMouseInWindow(
                    _window.SdlWindowHandle,
                    (int)_pendingCursorUpdate.Value.X,
                    (int)_pendingCursorUpdate.Value.Y);

                _pendingCursorUpdate = null;
            }
        }
        else PerfTracker.Skip();

        using (PerfTracker.FrameEvent("Raising preview input event"))
        {
            _previewEvent.DeltaSeconds = deltaSeconds;
            _previewEvent.Snapshot = snapshot;
            Raise(_previewEvent);
        }

        if (!_previewEvent.SuppressKeyboard)
        {
            using (PerfTracker.FrameEvent("Raising keyboard event"))
            {
                _keyboardEvent.DeltaSeconds = deltaSeconds;
                _keyboardEvent.KeyCharPresses = snapshot.KeyCharPresses;
                _keyboardEvent.KeyEvents = snapshot.KeyEvents;
                Raise(_keyboardEvent);
            }
        }
        else PerfTracker.Skip();

        if (!_previewEvent.SuppressMouse)
        {
            using (PerfTracker.FrameEvent("Raising mouse event"))
            {
                _mouseEvent.DeltaSeconds = deltaSeconds;
                _mouseEvent.Snapshot = snapshot;
                _mouseEvent.MouseDelta = _window.MouseDelta;
                Raise(_mouseEvent);
            }
        }
        else PerfTracker.Skip();
    }
}