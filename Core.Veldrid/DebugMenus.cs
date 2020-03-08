using System;
using ImGuiNET;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    class DebugMenus : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<DebugMenus, EngineUpdateEvent>((x, _) => x.RenderDebugMenu()));

        static readonly string[] MsaaOptions = { "Off", "2x", "4x", "8x", "16x", "32x" };
        readonly VeldridEngine _engine;
        int _msaaOption;

        public DebugMenus(VeldridEngine engine) : base(Handlers)
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
                        void BackendOption(string name, GraphicsBackend backend)
                        {
                            if (ImGui.MenuItem(
                                name,
                                string.Empty,
                                _engine.GraphicsDevice.BackendType == backend,
                                GraphicsDevice.IsBackendSupported(backend)))
                            {
                                Raise(new SetBackendEvent(GraphicsBackend.Vulkan));
                            }
                        }

                        BackendOption("Vulkan", GraphicsBackend.Vulkan);
                        BackendOption("OpenGL", GraphicsBackend.OpenGL);
                        BackendOption("OpenGL ES",GraphicsBackend.OpenGLES);
                        BackendOption("Direct3D 11",GraphicsBackend.Direct3D11);
                        BackendOption("Metal",GraphicsBackend.Metal);

                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("MSAA"))
                    {
                        if (ImGui.Combo("MSAA", ref _msaaOption, MsaaOptions, MsaaOptions.Length))
                            Raise(new SetMsaaLevelEvent(1 << _msaaOption));
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
