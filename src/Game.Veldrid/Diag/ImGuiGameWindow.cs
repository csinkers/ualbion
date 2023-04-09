using System;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Events;
using VeldridGen.Interfaces;

namespace UAlbion.Game.Veldrid.Diag;

public class ImGuiGameWindow : Component, IImGuiWindow
{
    readonly KeyboardInputEvent _keyboardEvent = new();
    readonly MouseInputEvent _mouseEvent = new();
    readonly IFramebufferHolder _framebuffer;
    readonly GameWindow _gameWindow;
    readonly string _name;
    bool _wasHovered;
    bool _dirty;

    public ImGuiGameWindow(int id, IFramebufferHolder framebuffer, GameWindow gameWindow)
    {
        _framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));
        _gameWindow = gameWindow ?? throw new ArgumentNullException(nameof(gameWindow));
        _name = $"Game###Game{id}";
    }

    protected override void Subscribed() => _dirty = true; // Make sure GameWindow gets resized when first displayed

    public void Draw()
    {
        var manager = Resolve<IImGuiManager>();
        var input = manager.LastInput;

        if (!manager.ConsumedKeyboard)
        {
            _keyboardEvent.DeltaSeconds = input.DeltaSeconds;
            _keyboardEvent.KeyCharPresses = input.Snapshot.KeyCharPresses;
            _keyboardEvent.KeyEvents = input.Snapshot.KeyEvents;
            Raise(_keyboardEvent);
            manager.ConsumedKeyboard = true;
        }

        ImGui.SetNextWindowSize(new Vector2(720 + 8, 480 + 8), ImGuiCond.FirstUseEver);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        bool open = true;
        ImGui.Begin(_name, ref open);

        var texture = _framebuffer.Framebuffer?.ColorTargets[0].Target;
        if (texture != null)
        {
            var handle = manager.GetOrCreateImGuiBinding(texture);
            ImGui.Image(handle, new Vector2(_framebuffer.Width, _framebuffer.Height));
        }

        var vMin = ImGui.GetWindowContentRegionMin();
        var vMax = ImGui.GetWindowContentRegionMax();
        var width = (int)(vMax.X - vMin.X);
        var height = (int)(vMax.Y - vMin.Y);

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
            _mouseEvent.IsMouseDown = input.Snapshot.IsMouseDown;
            Raise(_mouseEvent);
            manager.ConsumedMouse = true;
        }

        ImGui.End();
        ImGui.PopStyleVar();

        if (_dirty || width != _framebuffer.Width || height != _framebuffer.Height)
        {
            // Framebuffer requires resizing
            _framebuffer.Width = (uint)width;
            _framebuffer.Height = (uint)height;
            _gameWindow.Resize(width, height);

            _dirty = false;
        }

        if (!open)
            Remove();
    }
}