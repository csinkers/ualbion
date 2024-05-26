using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace UAlbion.Core.Veldrid;

sealed class WindowHolder : Component, IDisposable
{
    readonly InputEvent _inputEvent = new();
    Sdl2Window _window;
    DateTime _lastTitleUpdateTime;
    Vector2? _pendingCursorUpdate;
    WindowState _lastState;
    public Sdl2Window Window => _window;
    public string WindowTitle { get; set; }

    public WindowHolder()
    {
        On<SetCursorPositionEvent>(e    => _pendingCursorUpdate = new Vector2(e.X, e.Y));
        On<ToggleFullscreenEvent>(_     => ToggleFullscreenState());
        On<ShowHardwareCursorEvent>(e   => { if (_window != null) _window.CursorVisible = e.Show; });
        On<ToggleResizableEvent>(_      => { if (_window != null) _window.Resizable = !_window.Resizable; });
        On<ToggleVisibleBorderEvent>(_  => { if (_window != null) _window.BorderVisible = !_window.BorderVisible; });
        On<ConfineMouseToWindowEvent>(e => { if (_window != null) Sdl2Native.SDL_SetWindowGrab(_window.SdlWindowHandle, e.Enabled); });
        On<RenderSystemChangedEvent>(_  => { if (_window != null) Raise(new WindowResizedEvent(_window.Width, _window.Height)); });
        On<SetRelativeMouseModeEvent>(e =>
        {
            if (_window == null) return;
            Sdl2Native.SDL_SetRelativeMouseMode(e.Enabled);
            if (!e.Enabled)
                Sdl2Native.SDL_WarpMouseInWindow(_window.SdlWindowHandle, _window.Width / 2, _window.Height / 2);
        });
    }

    public void CreateWindow()
    {
        if (_window != null)
            return;

        var x = ReadVar(V.Core.Ui.WindowPosX);
        var y = ReadVar(V.Core.Ui.WindowPosY);
        var w = ReadVar(V.Core.Ui.WindowWidth);
        var h = ReadVar(V.Core.Ui.WindowHeight);

        var windowInfo = new WindowCreateInfo
        {
            X = x,
            Y = y,
            WindowWidth = w,
            WindowHeight = h,
            WindowInitialState = WindowState.Normal,
            WindowTitle = "UAlbion"
        };

        _window = VeldridStartup.CreateWindow(ref windowInfo);
        _window.Moved += pos =>
        {
            var settings = Resolve<ISettings>();
            V.Core.Ui.WindowPosX.Write(settings, pos.X);
            V.Core.Ui.WindowPosY.Write(settings, pos.Y);
        };

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

            var settings = Resolve<ISettings>();
            V.Core.Ui.WindowWidth.Write(settings, _window.Width);
            V.Core.Ui.WindowHeight.Write(settings, _window.Height);
            Raise(new WindowResizedEvent(_window.Width, _window.Height));
        };

        _window.Closed      += () => Raise(new WindowClosedEvent());
        _window.FocusGained += () => Raise(new FocusGainedEvent());
        _window.FocusLost   += () => Raise(new FocusLostEvent());
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

        using (PerfTracker.FrameEvent("Raising input event"))
        {
            _inputEvent.DeltaSeconds = deltaSeconds;
            _inputEvent.MouseDelta = _window.MouseDelta;
            _inputEvent.Snapshot = snapshot;
            Raise(_inputEvent);
        }
    }
}