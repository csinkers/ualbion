using System;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Game.Veldrid.Diag;

public class ImGuiGameWindow : Component, IImGuiWindow
{
    readonly KeyboardInputEvent _keyboardEvent = new();
    readonly MouseInputEvent _mouseEvent = new();
    readonly IFramebufferHolder _framebuffer;
    readonly GameWindow _gameWindow;
    bool _wasHovered;
    bool _dirty;

    public string Name { get; }
    public ImGuiGameWindow(string name, IFramebufferHolder framebuffer, GameWindow gameWindow)
    {
        Name = name;
        _framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));
        _gameWindow = gameWindow ?? throw new ArgumentNullException(nameof(gameWindow));
    }

    protected override void Subscribed() => _dirty = true; // Make sure GameWindow gets resized when first displayed

    public void Draw()
    {
        var manager = Resolve<IImGuiManager>();
        var input = manager.LastInput;

        if (!manager.ConsumedKeyboard)
        {
            _keyboardEvent.DeltaSeconds = input.DeltaSeconds;
            _keyboardEvent.InputEvents = input.Snapshot.InputEvents;
            _keyboardEvent.KeyEvents = input.Snapshot.KeyEvents;
            Raise(_keyboardEvent);
            manager.ConsumedKeyboard = true;
        }

        ImGui.SetNextWindowSize(new Vector2(720 + 8, 480 + 8), ImGuiCond.FirstUseEver);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        bool open = true;
        ImGui.Begin(Name, ref open);

        Vector2 contentRegion = ImGui.GetContentRegionAvail();
        var width = (int)contentRegion.X;
        var height = (int)contentRegion.Y;

        var texture = _framebuffer.Framebuffer?.ColorTargets[0].Target;
        if (texture != null)
        {
            var handle = manager.GetOrCreateImGuiBinding(texture);

            // Compensate for OpenGL's inverted texture coordinates compared to the other backends
            var engine = (IVeldridEngine)Resolve<IEngine>();
            if (engine.Device.BackendType is GraphicsBackend.OpenGL or GraphicsBackend.OpenGLES)
                ImGui.Image(handle, new Vector2(_framebuffer.Width, _framebuffer.Height), new Vector2(0, 1.0f), new Vector2(1.0f, 0));
            else
                ImGui.Image(handle, new Vector2(_framebuffer.Width, _framebuffer.Height));
        }

        var isHovered = ImGui.IsItemHovered();
        if (isHovered != _wasHovered)
        {
            Raise(new ShowHardwareCursorEvent(!isHovered));
            _wasHovered = isHovered;
        }

        if (isHovered)
        {
            var itemPos = ImGui.GetItemRectMin();
            _mouseEvent.DeltaSeconds = input.DeltaSeconds;
            _mouseEvent.MouseDelta = input.MouseDelta;
            _mouseEvent.WheelDelta = input.Snapshot.WheelDelta;
            _mouseEvent.MousePosition = input.Snapshot.MousePosition - itemPos;
            _mouseEvent.MouseEvents = input.Snapshot.MouseEvents;
            _mouseEvent.Snapshot = input.Snapshot;
            Raise(_mouseEvent);
            manager.ConsumedMouse = true;
        }

        ImGui.End();
        ImGui.PopStyleVar();

        if (_dirty || width != _framebuffer.Width || height != _framebuffer.Height)
        {
            if (width >= 1 && height >= 1) // Don't update if the content region gave a weird value
            {
                // Framebuffer requires resizing
                _framebuffer.Width = (uint)width;
                _framebuffer.Height = (uint)height;
                _gameWindow.Resize(width, height);

                _dirty = false;
            }
        }

        if (!open)
            Remove();
    }
}