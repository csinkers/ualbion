using System;
using Veldrid;

namespace UAlbion.Core.Veldrid;

public interface IImGuiTextureProvider
{
    IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, TextureView textureView);
    IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, Texture texture);
    void RemoveImGuiBinding(TextureView textureView);
    void RemoveImGuiBinding(Texture texture);
}