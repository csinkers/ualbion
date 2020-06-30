using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities
{
    public static class ItemTransition
    {
        static readonly Vector2 FirstPortraitPosition = new Vector2(23, 204);
        static readonly Vector2 ConversationPositionLeft = new Vector2(20, 20);
        static readonly Vector2 ConversationPositionRight = new Vector2(335, 20);
        const float DefaultTransitionTime = 0.35f;

        public static void LinearFromTilePosition(EventExchange exchange, int x, int y, ItemId itemId)
        {
            var scene = exchange.Resolve<ISceneManager>()?.ActiveScene;
            var map = exchange.Resolve<IMapManager>()?.Current;

            if (scene == null || map == null)
                return;

            var worldPosition = new Vector3(x, y, 0) * map.TileSize;
            var normPosition = scene.Camera.ProjectWorldToNorm(worldPosition);
            LinearFromNormPosition(
                exchange,
                new Vector2(normPosition.X, normPosition.Y),
                FirstPortraitPosition,
                itemId);
        }

        public static void LinearFromConversation(EventExchange exchange, ItemId itemId)
        {
            var wm = exchange.Resolve<IWindowManager>();
            var normPosition = wm.UiToNorm(ConversationPositionRight);
            LinearFromNormPosition(exchange, normPosition, ConversationPositionLeft, itemId);
        }

        public static void LinearFromNormPosition(EventExchange exchange, Vector2 fromNormPosition, Vector2 toUiPosition, ItemId itemId)
        {
            var assets = exchange.Resolve<IAssetManager>();
            var window = exchange.Resolve<IWindowManager>();

            var destPosition = window.UiToNorm(toUiPosition); // Tom's portrait, hardcoded for now.

            var item = assets.LoadItem(itemId);
            var icon = assets.LoadTexture(item.Icon).GetSubImageDetails((int)item.Icon);
            var size = window.UiToNormRelative(icon.Size);

            // Note: no need to attach as child as transitions clean themselves up.
            exchange.Attach(new LinearItemTransition<ItemSpriteId>(
                item.Icon, (int)item.Icon,
                fromNormPosition,
                destPosition,
                DefaultTransitionTime,
                size));
        }

        public static void GravityFromNormPosition(EventExchange exchange, Vector2 fromNormPosition, ItemId itemId)
        {
            var assets = exchange.Resolve<IAssetManager>();
            var window = exchange.Resolve<IWindowManager>();

            var item = assets.LoadItem(itemId);
            var icon = assets.LoadTexture(item.Icon).GetSubImageDetails((int)item.Icon);
            var size = window.UiToNormRelative(icon.Size);

            // Note: no need to attach as child as transitions clean themselves up.
            exchange.Attach(new GravityItemTransition<ItemSpriteId>(
                item.Icon, (int)item.Icon,
                fromNormPosition,
                size));
        }
    }
}
