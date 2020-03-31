using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Input;
using UAlbion.Game.Settings;
using UAlbion.Game.State;

namespace UAlbion.Game.Veldrid.Input
{
    public class CursorManager : Component, ICursorManager
    {
        CoreSpriteId _cursorId = CoreSpriteId.Cursor;
        public Vector2 Position { get; private set; }
        Vector2 _hotspot;
        Vector2 _size;
        SpriteLease _cursorSprite;
        SpriteLease _itemSprite;
        SpriteLease _hotspotSprite;
        bool _dirty = true;
        bool _showCursor = true;
        int _frame;

        public CursorManager() : base(Handlers) { }

        static readonly HandlerSet Handlers = new HandlerSet(
            H<CursorManager, InputEvent>((x, e) =>
            {
                x.Position = e.Snapshot.MousePosition;
                x._dirty = true;
            }),
            H<CursorManager, SetCursorPositionEvent>((x, e) =>
            {
                x.Position = new Vector2(e.X, e.Y);
                x._dirty = true;
            }),
            H<CursorManager, ClearInventoryItemInHandEvent>((x, e) => x._dirty = true),
            H<CursorManager, SetInventoryItemInHandEvent>((x, e) => x._dirty = true),
            H<CursorManager, RenderEvent>((x, e) => x.Render()),
            H<CursorManager, SlowClockEvent>((x, e) => x._frame += e.Delta),
            H<CursorManager, SetCursorEvent>((x, e) => x.SetCursor(e.CursorId)),
            H<CursorManager, ShowCursorEvent>((x, e) => { x._showCursor = e.Show; x._dirty = true; }),
            H<CursorManager, WindowResizedEvent>((x, e) => x.SetCursor(x._cursorId))
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
            var showHotspot = (Resolve<IDebugSettings>()?.DebugFlags ?? 0).HasFlag(DebugFlags.ShowCursorHotspot);
            if (showHotspot != (_hotspotSprite == null))
                _dirty = true;

            if (!_dirty)
                return;
            _dirty = false;

            var assets = Resolve<IAssetManager>();
            var sm = Resolve<ISpriteManager>();
            var window = Resolve<IWindowManager>();

            if (window.Size.X < 1 || window.Size.Y < 1)
                return;

            var position = new Vector3(window.PixelToNorm(Position - _hotspot), 1.0f);
            RenderCursor(assets, sm, window, position);
            RenderItemInHandCursor(assets, sm, window, position);
            RenderHotspot(sm, window, showHotspot);
        }

        void RenderHotspot(ISpriteManager sm, IWindowManager window, bool showHotspot)
        {
            if(!showHotspot)
            {
                _hotspotSprite?.Dispose();
                _hotspotSprite = null;
                return;
            }

            var commonColors = Resolve<ICommonColors>();
            if (_hotspotSprite == null)
            {
                var key = new SpriteKey(commonColors.BorderTexture, DrawLayer.MaxLayer, SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest);
                _hotspotSprite = sm.Borrow(key, 1, this);
            }

            var instances = _hotspotSprite.Access();
            var position = new Vector3(window.PixelToNorm(Position), 0.0f);
            var size = window.UiToNormRelative(Vector2.One);
            instances[0] = SpriteInstanceData.TopLeft(position, size, _hotspotSprite, (int)commonColors.Palette[CommonColor.Yellow3], 0);
        }

        void RenderCursor(IAssetManager assets, ISpriteManager sm, IWindowManager window, Vector3 position)
        {
            if (!_showCursor)
            {
                _cursorSprite?.Dispose();
                _itemSprite?.Dispose();
                _cursorSprite = null;
                _itemSprite = null;
                return;
            }

            var cursorTexture = assets.LoadTexture(_cursorId);
            if (cursorTexture == null)
                return;

            if (cursorTexture != _cursorSprite?.Key.Texture)
            {
                _cursorSprite?.Dispose();
                var key = new SpriteKey(cursorTexture, DrawLayer.Cursor, SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);
                _cursorSprite = sm.Borrow(key, 1, this);
            }

            var instances = _cursorSprite.Access();
            var size = window.UiToNormRelative(_size) / 2;
            instances[0] = SpriteInstanceData.TopMid(position, size, _cursorSprite, 0, 0);
        }

        void RenderItemInHandCursor(IAssetManager assets, ISpriteManager sm, IWindowManager window, Vector3 normPosition)
        {
            if (_cursorId != CoreSpriteId.CursorSmall) // Inventory screen, check what's being held.
            {
                _itemSprite?.Dispose();
                _itemSprite = null;
                return;
            }

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
            else if (held is ItemSlot itemInHand && itemInHand.Id.HasValue)
            {
                var item = assets.LoadItem(itemInHand.Id.Value);
                ItemSpriteId spriteId = item.Icon + _frame % item.IconAnim;
                texture = assets.LoadTexture(spriteId);
                subItem = (int)spriteId;
            }

            if (texture == null)
            {
                _itemSprite?.Dispose();
                _itemSprite = null;
                return;
            }

            if (texture != _itemSprite?.Key.Texture)
            {
                _itemSprite?.Dispose();

                var key = new SpriteKey(texture, DrawLayer.Cursor,
                    SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);

                _itemSprite = sm.Borrow(key, 1, this);
            }

            var subImage = texture.GetSubImageDetails(subItem);

            // TODO: Quantity text
            var instances = _itemSprite.Access();
            instances[0] = SpriteInstanceData.TopMid(
                normPosition + new Vector3(window.UiToNormRelative(6, 6), 0),
                window.UiToNormRelative(subImage.Size),
                subImage, 0);
        }
    }
}
