using Veldrid.Sdl2;

namespace UAlbion.Core.Veldrid
{
    internal class VeldridSdlWindow : IWindow
    {
        readonly Sdl2Window _window;

        public VeldridSdlWindow(Sdl2Window window)
        {
            _window = window;
        }

        public int Width => _window.Width;
        public int Height => _window.Height;
    }
}
