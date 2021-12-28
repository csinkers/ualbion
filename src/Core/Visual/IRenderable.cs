using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;

public interface IRenderable
{
    string Name { get; }
    DrawLayer RenderOrder { get; }
}