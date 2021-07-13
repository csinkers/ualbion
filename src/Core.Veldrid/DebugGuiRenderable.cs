using UAlbion.Api.Visual;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid
{
    public class DebugGuiRenderable : IRenderable
    {
        public static DebugGuiRenderable Instance { get; } = new();
        DebugGuiRenderable() { }
        public string Name => "DebugGui";
        public DrawLayer RenderOrder => DrawLayer.Debug;
    }
}