using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
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
        SpriteLease _cursorSprite;
        SpriteLease _itemSprite;
        bool _dirty = true;
        int _frame;

        public CursorManager() : base(Handlers) { }

        static readonly HandlerSet Handlers = new HandlerSet(
            H<CursorManager, InputEvent>((x,e) =>
            {
                x._position = e.Snapshot.MousePosition - x._hotspot;
                x._dirty = true;
            }),
            H<CursorManager, SetCursorPositionEvent>((x,e) =>
            {
                x._position = new Vector2(e.X, e.Y) - x._hotspot;
                x._dirty = true;
            }),
            H<CursorManager, ClearInventoryItemInHandEvent>((x, e) => x._dirty = true),
            H<CursorManager, SetInventoryItemInHandEvent>((x, e) => x._dirty = true),
            H<CursorManager, RenderEvent>((x,e) => x.Render()),
            H<CursorManager, SlowClockEvent>((x, e) => x._frame += e.Delta),
            H<CursorManager, SetCursorEvent>((x,e) => x.SetCursor(e.CursorId)),
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
            _dirty = true;
        }

        void Render()
        {
            if (!_dirty)
                return;
            _dirty = false;

            var assets = Resolve<IAssetManager>();
            var sm = Resolve<ISpriteManager>();
            var window = Resolve<IWindowManager>();

            if (window.Size.X < 1 || window.Size.Y < 1)
                return;

            var cursorTexture = assets.LoadTexture(_cursorId);
            if (cursorTexture == null)
                return;

            if (cursorTexture != _cursorSprite?.Key.Texture)
            {
                _cursorSprite?.Dispose();

                var key = new SpriteKey(cursorTexture, DrawLayer.MaxLayer,
                    SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);

                _cursorSprite = sm.Borrow(key, 1, this);
            }

            var instances = _cursorSprite.Access();
            var position = new Vector3(window.PixelToNorm(_position), 0.0f);
            var size = window.UiToNormRelative(_size) / 2;
            instances[0] = SpriteInstanceData.TopMid(position, size, _cursorSprite, 0, 0);

            if (_cursorId == CoreSpriteId.CursorSmall) // Inventory screen, check what's being held.
            {
                var held = Resolve<IInventoryScreenState>().ItemInHand;

                int subItem = 0;
                ITexture texture = null;

                if (held is GoldInHand)
                {
                    texture = assets.LoadTexture(CoreSpriteId.UiGold);
                }
                else if (held is RationsInHand)
                {
                    texture = assets.LoadTexture(CoreSpriteId.UiFood);
                }
                else if (held is ItemSlot itemInHand)
                {
                    var item = assets.LoadItem(itemInHand.Id);
                    ItemSpriteId spriteId = item.Icon + _frame % item.IconAnim;
                    texture = assets.LoadTexture(spriteId);
                    subItem = (int)spriteId;
                }

                if(texture == null)
                {
                    _itemSprite?.Dispose();
                    _itemSprite = null;
                    return;
                }

                if (texture != _itemSprite?.Key.Texture)
                {
                    _itemSprite?.Dispose();

                    var key = new SpriteKey(texture, DrawLayer.MaxLayer,
                        SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);

                    _itemSprite = sm.Borrow(key, 1, this);
                }

                texture.GetSubImageDetails(subItem, out size, out var to, out var ts, out var tl);

                // TODO: Quantity text
                instances = _itemSprite.Access();
                instances[0] = SpriteInstanceData.TopMid(
                    position + new Vector3(window.UiToNormRelative(new Vector2(6, 6)), 0),
                    window.UiToNormRelative(size),
                    to, ts, tl, 0);
            }
            else
            {
                _itemSprite?.Dispose();
                _itemSprite = null;
            }
        }
    }
}