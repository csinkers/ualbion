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

        static readonly HandlerSet Handlers = new HandlerSet(
            H<Skybox, RenderEvent>((x, e) =>
                {
                    if (x._renderable != null) e.Add(x._renderable);
                })
            );

        public Skybox(DungeonBackgroundId id) : base(Handlers) => _id = id;
        protected override void Subscribed()
        {
            var assets = Resolve<IAssetManager>();
            var texture = assets.LoadTexture(_id);
            _renderable = new SkyboxRenderable(texture);
            base.Subscribed();
        }
    }
}
