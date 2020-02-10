using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities
{
    public class Skybox : Component
    {
        readonly DungeonBackgroundId _id;
        TileMap _tilemap;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<Skybox, RenderEvent>((x, e) => x.Render(e)),
            H<Skybox, PostUpdateEvent>((x, _) => x.PostUpdate())
        );

        void PostUpdate()
        {
            _tilemap.Position = Resolve<ISceneManager>().ActiveScene.Camera.Position;
        }

        void Render(RenderEvent renderEvent)
        {
            renderEvent.Add(_tilemap);
        }

        public Skybox(DungeonBackgroundId id) : base(Handlers)
        {
            _id = id;
        }

        public override void Subscribed()
        {
            float size = 512.0f * 256.0f;
            var assets = Resolve<IAssetManager>();
            _tilemap = new TileMap(
                    "Skybox_" + _id,
                    DrawLayer.Background,
                    new Vector3(-size, size / 3, size), // Just make sure it's bigger than the largest map
                    1, 1,
                    Resolve<IPaletteManager>());
            var texture = assets.LoadTexture(_id);
            _tilemap.DefineWall(1, texture, 0, 0, 0, false);
            _tilemap.DefineWall(1, texture, texture.Width, 0, 0, false);
            _tilemap.DefineWall(1, texture, texture.Width * 2, 0, 0, false);
            _tilemap.Set(0, 0, 0, 0, 0, 1, 0);
        }
    }
}