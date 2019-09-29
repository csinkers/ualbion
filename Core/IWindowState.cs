using System.Numerics;

namespace UAlbion.Core
{
    public interface IWindowState
    {
        int Width { get; }
        int Height { get; }
        Vector2 Size { get; }
        int GuiScale { get; }
    }
}