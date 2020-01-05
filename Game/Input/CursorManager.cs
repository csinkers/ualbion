using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Input
{
    public class CursorManager : Component
    {
        CoreSpriteId _cursorId = CoreSpriteId.Cursor;
        Vector2 _position;
        Vector2 _hotspot;
        Vector2 _size;

        public CursorManager() : base(Handlers) { }

        static readonly HandlerSet Handlers = new HandlerSet(
            H<CursorManager, InputEvent>((x,e) => x._position = e.Snapshot.MousePosition - x._hotspot),
            H<CursorManager, RenderEvent>((x,e) => x.Render(e)),
            H<CursorManager, SetCursorEvent>((x,e) => x.SetCursor(e.CursorId)),
            H<CursorManager, SetCursorPositionEvent>((x,e) => x._position = new Vector2(e.X, e.Y) - x._hotspot),
            H<CursorManager, WindowResizedEvent>((x,e) => x.SetCursor(x._cursorId))
        );

        void SetCursor(CoreSpriteId id)
        {
            var assets = Resolve<IAssetManager>();
            var window = Resolve<IWindowManager>();
            var texture = assets.LoadTexture(id);
            var config = assets.LoadCoreSpriteInfo(id);
            _cursorId = id;
            _size = new Vector2(texture.Width, texture.Height);
            _hotspot = config.Hotspot == null 
                ? Vector2.Zero
                : window.GuiScale * new Vector2(config.Hotspot.X, config.Hotspot.Y);
        }

        void Render(RenderEvent e)
        {
            var window = Resolve<IWindowManager>();
            if (window.Size.X < 1 || window.Size.Y < 1)
                return;

            var position = new Vector3(window.PixelToNorm(_position), 0.0f);
            var size = new Vector2(window.GuiScale, -window.GuiScale) * _size / window.Size;

            e.Add(new Sprite<CoreSpriteId>(_cursorId,
                0,
                position,
                (int)DrawLayer.MaxLayer,
                SpriteFlags.NoTransform | SpriteFlags.NoDepthTest,
                size));

            if (_cursorId == CoreSpriteId.CursorSmall) // Inventory screen, check what's being held.
            {
                var assets = Resolve<IAssetManager>();
                var state = Resolve<IStateManager>();
                var held = state.State.InventoryScreenState.ItemInHand;
                if (held is GoldInHand)
                {
                    var spriteId = CoreSpriteId.UiGold;
                    var texture = assets.LoadTexture(spriteId);
                    texture.GetSubImageDetails(0, out var itemSize, out _, out _, out _);
                    e.Add(BuildItemInHandSprite(spriteId, itemSize, position));
                }
                else if (held is RationsInHand)
                {
                    var spriteId = CoreSpriteId.UiFood;
                    var texture = assets.LoadTexture(spriteId);
                    texture.GetSubImageDetails(0, out var itemSize, out _, out _, out _);
                    e.Add(BuildItemInHandSprite(spriteId, itemSize, position));
                }
                else if (held is ItemSlot itemInHand)
                {
                    var item = assets.LoadItem(itemInHand.Id);
                    ItemSpriteId spriteId = item.Icon + state.FrameCount % item.IconAnim;
                    var texture = assets.LoadTexture(spriteId);
                    texture.GetSubImageDetails((int)spriteId, out var itemSize, out _, out _, out _);
                    e.Add(new Sprite<ItemSpriteId>(spriteId,
                        (int)spriteId,
                        position + new Vector3(window.UiToNormRelative(new Vector2(6, 0)), 0),
                        (int)DrawLayer.MaxLayer,
                        SpriteFlags.NoTransform | SpriteFlags.NoDepthTest,
                        window.UiToNormRelative(itemSize)));
                    // TODO: Quantity text
                }
            }
        }

        IRenderable BuildItemInHandSprite<T>(T spriteId, Vector2 size, Vector3 position) where T : Enum
        {
            var window = Resolve<IWindowManager>();
            return new Sprite<T>(spriteId,
                0,
                position + new Vector3(window.UiToNormRelative(new Vector2(6, 6)), 0),
                (int)DrawLayer.MaxLayer,
                SpriteFlags.NoTransform | SpriteFlags.NoDepthTest,
                window.UiToNormRelative(size));
        }
    }
}