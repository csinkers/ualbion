using UAlbion.Api;

namespace UAlbion.Core.Visual
{
    public interface IRenderable
    {
        string Name { get; }
        DrawLayer RenderOrder { get; }
        int PipelineId { get; }
    }
}
