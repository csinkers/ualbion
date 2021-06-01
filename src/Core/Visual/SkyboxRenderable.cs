using System;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual
{
    public class SkyboxRenderable : IRenderable
    {
        public SkyboxRenderable(ITexture texture) => Texture = texture ?? throw new ArgumentNullException(nameof(texture));
        public ITexture Texture { get; }
        public string Name => Texture.Name;
        public DrawLayer RenderOrder => DrawLayer.Background;
        public int PipelineId => 0;
    }
}