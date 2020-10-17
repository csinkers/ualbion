using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities
{
    public class Skybox : Component
    {
        readonly SpriteId _id;
        SkyboxRenderable _renderable;

        public Skybox(SpriteId id)
        {
            On<RenderEvent>(e =>
            {
                if (_renderable != null)
                    e.Add(_renderable);
            });

            _id = id;
        }

        protected override void Subscribed()
        {
            var assets = Resolve<IAssetManager>();
            var texture = assets.LoadTexture(_id);
            _renderable = new SkyboxRenderable(texture);
        }
    }
}
