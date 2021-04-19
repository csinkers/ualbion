using System;
using ImGuiNET;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public class DebugMenus : Component
    {
        static readonly string[] MsaaOptions = { "Off", "2x", "4x", "8x", "16x", "32x" };
        readonly VeldridEngine _engine;
        int _msaaOption;

        public DebugMenus(VeldridEngine engine)
        {
            On<EngineUpdateEvent>(_ => RenderDebugMenu());
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

                if (ImGui.BeginMenu("RenderDoc"))
                {
                    if (VeldridEngine.RenderDoc == null)
                    {
                        if (ImGui.MenuItem("Load"))
                            Raise(new LoadRenderDocEvent());
                    }
                    else
                    {
                        if (ImGui.MenuItem("Trigger Capture"))
                            VeldridEngine.RenderDoc.TriggerCapture();

                        if (ImGui.BeginMenu("Options"))
                        {
                            bool allowVsync = VeldridEngine.RenderDoc.AllowVSync;
                            if (ImGui.Checkbox("Allow VSync", ref allowVsync))
                                VeldridEngine.RenderDoc.AllowVSync = allowVsync;

                            bool validation = VeldridEngine.RenderDoc.APIValidation;
                            if (ImGui.Checkbox("API Validation", ref validation))
                                VeldridEngine.RenderDoc.APIValidation = validation;

                            int delayForDebugger = (int)VeldridEngine.RenderDoc.DelayForDebugger;
                            if (ImGui.InputInt("Debugger Delay", ref delayForDebugger))
                            {
                                delayForDebugger = Math.Clamp(delayForDebugger, 0, int.MaxValue);
                                VeldridEngine.RenderDoc.DelayForDebugger = (uint)delayForDebugger;
                            }

                            bool verifyBufferAccess = VeldridEngine.RenderDoc.VerifyBufferAccess;
                            if (ImGui.Checkbox("Verify Buffer Access", ref verifyBufferAccess))
                                VeldridEngine.RenderDoc.VerifyBufferAccess = verifyBufferAccess;

                            bool overlayEnabled = VeldridEngine.RenderDoc.OverlayEnabled;
                            if (ImGui.Checkbox("Overlay Visible", ref overlayEnabled))
                                VeldridEngine.RenderDoc.OverlayEnabled = overlayEnabled;

                            bool overlayFrameRate = VeldridEngine.RenderDoc.OverlayFrameRate;
                            if (ImGui.Checkbox("Overlay Frame Rate", ref overlayFrameRate))
                                VeldridEngine.RenderDoc.OverlayFrameRate = overlayFrameRate;

                            bool overlayFrameNumber = VeldridEngine.RenderDoc.OverlayFrameNumber;
                            if (ImGui.Checkbox("Overlay Frame Number", ref overlayFrameNumber))
                                VeldridEngine.RenderDoc.OverlayFrameNumber = overlayFrameNumber;

                            bool overlayCaptureList = VeldridEngine.RenderDoc.OverlayCaptureList;
                            if (ImGui.Checkbox("Overlay Capture List", ref overlayCaptureList))
                                VeldridEngine.RenderDoc.OverlayCaptureList = overlayCaptureList;

                            ImGui.EndMenu();
                        }

                        if (ImGui.MenuItem("Launch Replay UI"))
                            VeldridEngine.RenderDoc.LaunchReplayUI();
                    }
                    ImGui.EndMenu();
                }

                ImGui.Text(_engine.FrameTimeText);
                ImGui.EndMainMenuBar();
            }
        }
    }
}
