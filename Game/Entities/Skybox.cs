using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Entities
{
    public class Skybox : Component
    {
        readonly DungeonBackgroundId _id;
        SkyboxRenderable _renderable;

        public Skybox(DungeonBackgroundId id)
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
