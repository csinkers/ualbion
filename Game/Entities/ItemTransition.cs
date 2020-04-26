using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities
{
    public class ItemTransition<T> : Component where T : Enum
    {
        readonly Vector2 _fromPosition;
        readonly Vector2 _toPosition;
        readonly float _transitionTimeSeconds;
        readonly Sprite<T> _sprite;
        float _elapsedTime;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<ItemTransition<T>, EngineUpdateEvent>((x,e) => x.Update(e.DeltaSeconds))
        );

        public static void CreateTransitionFromTilePosition(EventExchange exchange, int x, int y, ItemId itemId)
        {
            var scene = exchange.Resolve<ISceneManager>()?.ActiveScene;
            var map = exchange.Resolve<IMapManager>()?.Current;

            if (scene == null || map == null)
                return;

            var worldPosition = new Vector3(x, y, 0) * map.TileSize;
            var normPosition = scene.Camera.ProjectWorldToNorm(worldPosition);
            CreateTransitionFromNormPosition(exchange, normPosition, itemId);
        }

        public static void CreateTransitionFromNormPosition(EventExchange exchange, Vector3 normPosition, ItemId itemId)
        {
            var assets = exchange.Resolve<IAssetManager>();
            var window = exchange.Resolve<IWindowManager>();

            var destPosition = window.UiToNorm(23, 204); // Tom's portrait, hardcoded for now.

            var item = assets.LoadItem(itemId);
            var icon = assets.LoadTexture(item.Icon).GetSubImageDetails((int)item.Icon);
            var size = window.UiToNormRelative(icon.Size);

            var transition = new ItemTransition<ItemSpriteId>(
                item.Icon, (int)item.Icon,
                new Vector2(normPosition.X, normPosition.Y),
                destPosition,
                0.3f, size);

            exchange.Attach(transition); // No need to attach as child as transitions clean themselves up.
        }

        public ItemTransition(T spriteId, int subImage, Vector2 fromPosition, Vector2 toPosition, float transitionTimeSeconds, Vector2 size) : base(Handlers)
        {
            _fromPosition = fromPosition;
            _toPosition = toPosition;
            _transitionTimeSeconds = transitionTimeSeconds;

            _sprite = AttachChild(new Sprite<T>(
                spriteId,
                new Vector3(fromPosition, 0),
                DrawLayer.InterfaceOverlay,
                SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest,
                SpriteFlags.LeftAligned)
            {
                Size = size,
                Frame = subImage
            });
        }

        void Update(float deltaSeconds)
        {
            _elapsedTime += deltaSeconds;
            float t = _elapsedTime / _transitionTimeSeconds;
            if (t > 1.0f)
            {
                Detach();
                return;
            }

            _sprite.Position = new Vector3(Vector2.Lerp(_fromPosition, _toPosition, t), 0);
        }
    }
}
