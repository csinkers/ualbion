using System;
using System.Collections.Generic;
using UAlbion.Core.Veldrid.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid;

public interface IImGuiManager
{
    int GetNextWindowId();
    void AddWindow(IImGuiWindow window);
    IEnumerable<IImGuiWindow> FindWindows(string prefix);
    IntPtr GetOrCreateImGuiBinding(TextureView textureView);
    IntPtr GetOrCreateImGuiBinding(Texture texture);
    void RemoveImGuiBinding(TextureView textureView);
    void RemoveImGuiBinding(Texture texture);
    InputEvent LastInput { get; }
    bool ConsumedKeyboard { get; set; }
    bool ConsumedMouse { get; set; }
}