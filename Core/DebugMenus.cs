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
                        bool Selected(GraphicsBackend b) => _engine.GraphicsDevice.BackendType == b;
                        bool Enabled(GraphicsBackend b) => GraphicsDevice.IsBackendSupported(b);

                        if (ImGui.MenuItem("Vulkan", string.Empty, Selected(GraphicsBackend.Vulkan), Enabled(GraphicsBackend.Vulkan))) _engine.ChangeBackend(GraphicsBackend.Vulkan);
                        if (ImGui.MenuItem("OpenGL", string.Empty, Selected(GraphicsBackend.OpenGL), Enabled(GraphicsBackend.OpenGL))) _engine.ChangeBackend(GraphicsBackend.OpenGL);
                        if (ImGui.MenuItem("OpenGL ES", string.Empty, Selected(GraphicsBackend.OpenGLES), Enabled(GraphicsBackend.OpenGLES))) _engine.ChangeBackend(GraphicsBackend.OpenGLES);
                        if (ImGui.MenuItem("Direct3D 11", string.Empty, Selected(GraphicsBackend.Direct3D11), Enabled(GraphicsBackend.Direct3D11))) _engine.ChangeBackend(GraphicsBackend.Direct3D11);
                        if (ImGui.MenuItem("Metal", string.Empty, Selected(GraphicsBackend.Metal), Enabled(GraphicsBackend.Metal))) _engine.ChangeBackend(GraphicsBackend.Metal);

                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("MSAA"))
                    {
                        if (ImGui.Combo("MSAA", ref _msaaOption, MsaaOptions, MsaaOptions.Length))
                            _engine.ChangeMsaa(_msaaOption);
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Window"))
                {
                    /*
                    bool isFullscreen = _engine.Window.WindowState == WindowState.BorderlessFullScreen;
                    if (ImGui.MenuItem("Fullscreen", "F11", isFullscreen, true))
                        Raise(new ToggleFullscreenEvent());
                        */

                    if (ImGui.MenuItem("VSync", string.Empty, _engine.GraphicsDevice.SyncToVerticalBlank))
                        _engine.GraphicsDevice.SyncToVerticalBlank = !_engine.GraphicsDevice.SyncToVerticalBlank;
                    /*
                    if (ImGui.MenuItem("Resizable Window", string.Empty, _engine.Window.Resizable))
                        Raise(new ToggleResizableEvent());

                    if (ImGui.MenuItem("Visible Window Border", string.Empty, _engine.Window.BorderVisible))
                        Raise(new ToggleVisibleBorderEvent());
                    */

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Debug"))
                {
                    if (ImGui.MenuItem("Refresh Device Objects"))
                        _engine.RefreshDeviceObjects(1);
                    if (ImGui.MenuItem("Refresh Device Objects (10 times)"))
                        _engine.RefreshDeviceObjects(10);
                    if (ImGui.MenuItem("Refresh Device Objects (100 times)"))
                        _engine.RefreshDeviceObjects(100);

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("RenderDoc"))
                {
                    if (_engine.RenderDoc == null)
                    {
                        if (ImGui.MenuItem("Load"))
                            Raise(new LoadRenderDocEvent());
                    }
                    else
                    {
                        if (ImGui.MenuItem("Trigger Capture"))
                            _engine.RenderDoc.TriggerCapture();

                        if (ImGui.BeginMenu("Options"))
                        {
                            bool allowVsync = _engine.RenderDoc.AllowVSync;
                            if (ImGui.Checkbox("Allow VSync", ref allowVsync))
                                _engine.RenderDoc.AllowVSync = allowVsync;

                            bool validation = _engine.RenderDoc.APIValidation;
                            if (ImGui.Checkbox("API Validation", ref validation))
                                _engine.RenderDoc.APIValidation = validation;

                            int delayForDebugger = (int)_engine.RenderDoc.DelayForDebugger;
                            if (ImGui.InputInt("Debugger Delay", ref delayForDebugger))
                            {
                                delayForDebugger = Math.Clamp(delayForDebugger, 0, int.MaxValue);
                                _engine.RenderDoc.DelayForDebugger = (uint)delayForDebugger;
                            }

                            bool verifyBufferAccess = _engine.RenderDoc.VerifyBufferAccess;
                            if (ImGui.Checkbox("Verify Buffer Access", ref verifyBufferAccess))
                                _engine.RenderDoc.VerifyBufferAccess = verifyBufferAccess;

                            bool overlayEnabled = _engine.RenderDoc.OverlayEnabled;
                            if (ImGui.Checkbox("Overlay Visible", ref overlayEnabled))
                                _engine.RenderDoc.OverlayEnabled = overlayEnabled;

                            bool overlayFrameRate = _engine.RenderDoc.OverlayFrameRate;
                            if (ImGui.Checkbox("Overlay Frame Rate", ref overlayFrameRate))
                                _engine.RenderDoc.OverlayFrameRate = overlayFrameRate;

                            bool overlayFrameNumber = _engine.RenderDoc.OverlayFrameNumber;
                            if (ImGui.Checkbox("Overlay Frame Number", ref overlayFrameNumber))
                                _engine.RenderDoc.OverlayFrameNumber = overlayFrameNumber;

                            bool overlayCaptureList = _engine.RenderDoc.OverlayCaptureList;
                            if (ImGui.Checkbox("Overlay Capture List", ref overlayCaptureList))
                                _engine.RenderDoc.OverlayCaptureList = overlayCaptureList;

                            ImGui.EndMenu();
                        }

                        if (ImGui.MenuItem("Launch Replay UI"))
                            _engine.RenderDoc.LaunchReplayUI();
                    }
                    ImGui.EndMenu();
                }

                ImGui.Text(_engine.FrameTimeText);
                ImGui.EndMainMenuBar();
            }
        }
    }
}
