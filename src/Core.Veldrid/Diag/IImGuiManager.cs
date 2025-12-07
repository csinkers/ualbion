using System.Collections.Generic;
using UAlbion.Core.Veldrid.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid.Diag;

public interface IImGuiManager
{
    int GetNextWindowId();
    void AddWindow(IImGuiWindow window);
    void CloseAllWindows();
    void SaveSettings();
    IEnumerable<IImGuiWindow> FindWindows(string prefix);
    nint GetOrCreateImGuiBinding(TextureView textureView);
    nint GetOrCreateImGuiBinding(Texture texture);
    void RemoveImGuiBinding(TextureView textureView);
    void RemoveImGuiBinding(Texture texture);
    InputEvent LastInput { get; }
    bool ConsumedKeyboard { get; set; }
    bool ConsumedMouse { get; set; }
}