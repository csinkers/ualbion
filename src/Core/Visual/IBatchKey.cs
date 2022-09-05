using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;

public interface IBatchKey
{
    DrawLayer RenderOrder { get; }
}