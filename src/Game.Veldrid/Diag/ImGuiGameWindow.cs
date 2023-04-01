using System;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Game.Veldrid.Diag;

public class ImGuiGameWindow : Component, IImGuiWindow
{
    readonly IFramebufferHolder _framebuffer;
    readonly GameWindow _gameWindow;
    readonly string _name;
    bool _dirty;

    public ImGuiGameWindow(int id, IFramebufferHolder framebuffer, GameWindow gameWindow)
    {
        _framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));
        _gameWindow = gameWindow ?? throw new ArgumentNullException(nameof(gameWindow));
        _name = $"Game###Game{id}";
    }

    protected override void Subscribed() => _dirty = true; // Make sure GameWindow gets resized when first displayed

    public void Draw(GraphicsDevice device)
    {
        var textureProvider = Resolve<IImGuiTextureProvider>();
        var texture = _framebuffer.Framebuffer.ColorTargets[0].Target;
        var handle = textureProvider.GetOrCreateImGuiBinding(device.ResourceFactory, texture);

        ImGui.SetNextWindowSize(new Vector2(720 + 8, 480 + 8), ImGuiCond.FirstUseEver);
        ImGui.Begin(_name);
        ImGui.Image(handle, new Vector2(_framebuffer.Width, _framebuffer.Height));
        var contentSize = ImGui.GetWindowContentRegionMax();
        ImGui.End();

        if (_dirty ||  (int)contentSize.X != _framebuffer.Width || (int)contentSize.Y != _framebuffer.Height)
        {
            // Framebuffer requires resizing
            _framebuffer.Width = (uint)contentSize.X;
            _framebuffer.Height = (uint)contentSize.Y;
            _gameWindow.Resize((int)contentSize.X, (int)contentSize.Y);
            _dirty = false;
        }
    }
}