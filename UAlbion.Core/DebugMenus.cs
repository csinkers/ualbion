using System;
using System.Collections.Generic;
using ImGuiNET;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core
{
    class DebugMenus : Component
    {
        static readonly IList<Handler> Handlers = new Handler[] { new Handler<DebugMenus, EngineUpdateEvent>((x, _) => x.RenderDebugMenu()) };
        static readonly string[] MsaaOptions = { "Off", "2x", "4x", "8x", "16x", "32x" };
        readonly Engine _engine;
        int _msaaOption = 0;

        public DebugMenus(Engine engine) : base(Handlers)
        {
            _engine = engine;
        }

        void RenderDebugMenu()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Settings"))
                {
                    if (ImGui.BeginMenu("Graphics Backend"))
                    {

                        if (ImGui.MenuItem("Vulkan", string.Empty, _engine.GraphicsDevice.BackendType == GraphicsBackend.Vulkan, GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan)))
                        {
                            _engine.ChangeBackend(GraphicsBackend.Vulkan);
                        }
                        if (ImGui.MenuItem("OpenGL", string.Empty, _engine.GraphicsDevice.BackendType == GraphicsBackend.OpenGL, GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGL)))
                        {
                            _engine.ChangeBackend(GraphicsBackend.OpenGL);
                        }
                        if (ImGui.MenuItem("OpenGL ES", string.Empty, _engine.GraphicsDevice.BackendType == GraphicsBackend.OpenGLES, GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGLES)))
                        {
                            _engine.ChangeBackend(GraphicsBackend.OpenGLES);
                        }
                        if (ImGui.MenuItem("Direct3D 11", string.Empty, _engine.GraphicsDevice.BackendType == GraphicsBackend.Direct3D11, GraphicsDevice.IsBackendSupported(GraphicsBackend.Direct3D11)))
                        {
                            _engine.ChangeBackend(GraphicsBackend.Direct3D11);
                        }
                        if (ImGui.MenuItem("Metal", string.Empty, _engine.GraphicsDevice.BackendType == GraphicsBackend.Metal, GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal)))
                        {
                            _engine.ChangeBackend(GraphicsBackend.Metal);
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu("MSAA"))
                    {
                        if (ImGui.Combo("MSAA", ref _msaaOption, MsaaOptions, MsaaOptions.Length))
                        {
                            _engine.ChangeMsaa(_msaaOption);
                        }

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Window"))
                {
                    bool isFullscreen = _engine.Window.WindowState == WindowState.BorderlessFullScreen;
                    if (ImGui.MenuItem("Fullscreen", "F11", isFullscreen, true))
                    {
                        Raise(new ToggleFullscreenEvent());
                    }
                    //if (ImGui.MenuItem("Always Recreate Sdl2Window", string.Empty, _recreateWindow, true))
                    //{
                    //    _recreateWindow = !_recreateWindow;
                    //}
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(
                            "Causes a new OS window to be created whenever the graphics backend is switched. This is much safer, and is the default.");
                    }

                    bool vsync = _engine.GraphicsDevice.SyncToVerticalBlank;
                    if (ImGui.MenuItem("VSync", string.Empty, vsync, true))
                    {
                        _engine.GraphicsDevice.SyncToVerticalBlank = !_engine.GraphicsDevice.SyncToVerticalBlank;
                    }

                    bool resizable = _engine.Window.Resizable;
                    if (ImGui.MenuItem("Resizable Window", string.Empty, resizable))
                        Raise(new ToggleResizableEvent());

                    bool bordered = _engine.Window.BorderVisible;
                    if (ImGui.MenuItem("Visible Window Border", string.Empty, bordered))
                        Raise(new ToggleVisibleBorderEvent());

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Debug"))
                {
                    if (ImGui.MenuItem("Refresh Device Objects"))
                    {
                        _engine.RefreshDeviceObjects(1);
                    }
                    if (ImGui.MenuItem("Refresh Device Objects (10 times)"))
                    {
                        _engine.RefreshDeviceObjects(10);
                    }
                    if (ImGui.MenuItem("Refresh Device Objects (100 times)"))
                    {
                        _engine.RefreshDeviceObjects(100);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("RenderDoc"))
                {
                    if (_engine.RenderDoc == null)
                    {
                        if (ImGui.MenuItem("Load"))
                        {
                            Raise(new LoadRenderDocEvent());
                        }
                    }
                    else
                    {
                        if (ImGui.MenuItem("Trigger Capture"))
                        {
                            _engine.RenderDoc.TriggerCapture();
                        }
                        if (ImGui.BeginMenu("Options"))
                        {
                            bool allowVsync = _engine.RenderDoc.AllowVSync;
                            if (ImGui.Checkbox("Allow VSync", ref allowVsync))
                            {
                                _engine.RenderDoc.AllowVSync = allowVsync;
                            }
                            bool validation = _engine.RenderDoc.APIValidation;
                            if (ImGui.Checkbox("API Validation", ref validation))
                            {
                                _engine.RenderDoc.APIValidation = validation;
                            }
                            int delayForDebugger = (int)_engine.RenderDoc.DelayForDebugger;
                            if (ImGui.InputInt("Debugger Delay", ref delayForDebugger))
                            {
                                delayForDebugger = Math.Clamp(delayForDebugger, 0, int.MaxValue);
                                _engine.RenderDoc.DelayForDebugger = (uint)delayForDebugger;
                            }
                            bool verifyBufferAccess = _engine.RenderDoc.VerifyBufferAccess;
                            if (ImGui.Checkbox("Verify Buffer Access", ref verifyBufferAccess))
                            {
                                _engine.RenderDoc.VerifyBufferAccess = verifyBufferAccess;
                            }
                            bool overlayEnabled = _engine.RenderDoc.OverlayEnabled;
                            if (ImGui.Checkbox("Overlay Visible", ref overlayEnabled))
                            {
                                _engine.RenderDoc.OverlayEnabled = overlayEnabled;
                            }
                            bool overlayFrameRate = _engine.RenderDoc.OverlayFrameRate;
                            if (ImGui.Checkbox("Overlay Frame Rate", ref overlayFrameRate))
                            {
                                _engine.RenderDoc.OverlayFrameRate = overlayFrameRate;
                            }
                            bool overlayFrameNumber = _engine.RenderDoc.OverlayFrameNumber;
                            if (ImGui.Checkbox("Overlay Frame Number", ref overlayFrameNumber))
                            {
                                _engine.RenderDoc.OverlayFrameNumber = overlayFrameNumber;
                            }
                            bool overlayCaptureList = _engine.RenderDoc.OverlayCaptureList;
                            if (ImGui.Checkbox("Overlay Capture List", ref overlayCaptureList))
                            {
                                _engine.RenderDoc.OverlayCaptureList = overlayCaptureList;
                            }
                            ImGui.EndMenu();
                        }
                        if (ImGui.MenuItem("Launch Replay UI"))
                        {
                            _engine.RenderDoc.LaunchReplayUI();
                        }
                    }
                    ImGui.EndMenu();
                }

                ImGui.Text(_engine.FrameTimeText);
                ImGui.EndMainMenuBar();
            }
        }
    }
}
