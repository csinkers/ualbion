using System.Globalization;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Input;
using UAlbion.Game.Settings;
using UAlbion.Game.State.Player;
using UAlbion.Game.Text;

namespace UAlbion.Game.Veldrid.Input
{
    public class CursorManager : ServiceComponent<ICursorManager>, ICursorManager
    {
        SpriteId _cursorId = Base.CoreSprite.Cursor;

        public Vector2 Position { get; private set; }
        Vector2 _hotspot;
        ISpriteLease _cursorSprite;
        ISpriteLease _itemSprite;
        ISpriteLease _hotspotSprite;
        PositionedSpriteBatch _itemAmountSprite;
        string _lastItemAmountText;
        bool _dirty = true;
        bool _showCursor = true;
        bool _relative;
        int _frame;

        public CursorManager()
        {
            On<RenderEvent>(e => Render());
            On<IdleClockEvent>(e => _frame++);
            On<SetCursorEvent>(e => SetCursor(e.CursorId));
            On<ShowCursorEvent>(e => { _showCursor = e.Show; _dirty = true; });
            On<WindowResizedEvent>(e => SetCursor(_cursorId));
            On<SetRelativeMouseModeEvent>(e => _relative = e.Enabled);
            On<InputEvent>(e =>
            {
                if (_relative)
                {
                    var windowState = Resolve<IWindowManager>();
                    Position = new Vector2((int)(windowState.PixelWidth / 2), (int)(windowState.PixelHeight / 2));
                }
                else Position = e.Snapshot.MousePosition;
                _dirty = true;
            });
            On<SetCursorPositionEvent>(e =>
            {
                Position = new Vector2(e.X, e.Y);
                _dirty = true;
            });
        }

        void SetCursor(SpriteId cursorId)
        {
            var assets = Resolve<IAssetManager>();
            var window = Resolve<IWindowManager>();
            var config = assets.GetAssetInfo(cursorId);

            _cursorId = cursorId;
            var hotspot = config?.GetRaw("Hotspot");
            _hotspot = hotspot == null
                ? Vector2.Zero
                : window.GuiScale * new Vector2(hotspot.Value<int>("X"), hotspot.Value<int>("Y"));
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
            var factory = Resolve<ICoreFactory>();
            var window = Resolve<IWindowManager>();

            if (window.Size.X < 1 || window.Size.Y < 1)
                return;

            var position = new Vector3(window.PixelToNorm(Position - _hotspot), 1.0f);
            RenderCursor(assets, factory, window, position);
            RenderItemInHandCursor(assets, factory, window, position);
            RenderHotspot(factory, window, showHotspot);
        }

        void RenderHotspot(ICoreFactory factory, IWindowManager window, bool showHotspot)
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
                var key = new SpriteKey(commonColors.BorderTexture, SpriteSampler.Point, DrawLayer.MaxLayer, SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest);
                _hotspotSprite = factory.CreateSprites(key, 1, this);
            }

            var position = new Vector3(window.PixelToNorm(Position), 0.0f);
            var size = window.UiToNormRelative(Vector2.One);

            bool lockWasTaken = false;
            var instances = _hotspotSprite.Lock(ref lockWasTaken);
            try
            {
                var region = _hotspotSprite.Key.Texture.Regions[(int) commonColors.Palette[CommonColor.Yellow3]];
                instances[0] = new SpriteInstanceData(position, size, region, SpriteFlags.TopMid);
            }
            finally { _hotspotSprite.Unlock(lockWasTaken); }
        }

        void RenderCursor(IAssetManager assets, ICoreFactory factory, IWindowManager window, Vector3 position)
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
                var key = new SpriteKey(cursorTexture, SpriteSampler.Point, DrawLayer.Cursor, SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);
                _cursorSprite = factory.CreateSprites(key, 1, this);
            }

            var size = window.UiToNormRelative(new Vector2(cursorTexture.Width, cursorTexture.Height));

            bool lockWasTaken = false;
            var instances = _cursorSprite.Lock(ref lockWasTaken);
            try
            {
                instances[0] = new SpriteInstanceData(position, size, _cursorSprite.Key.Texture.Regions[0], SpriteFlags.TopMid);
            }
            finally { _cursorSprite.Unlock(lockWasTaken); }
        }

        void RenderItemInHandCursor(IAssetManager assets, ICoreFactory factory, IWindowManager window, Vector3 normPosition)
        {
            var held = Resolve<IInventoryManager>().ItemInHand;
            var itemAmountText = GetAmountText();

            if (_lastItemAmountText != itemAmountText)
            {
                var tm = Resolve<ITextManager>();
                _lastItemAmountText = itemAmountText;
                _itemAmountSprite?.Dispose();
                _itemAmountSprite = itemAmountText == null 
                    ? null 
                    : tm.BuildRenderable(new TextBlock(itemAmountText), DrawLayer.MaxLayer, null, this);
            }

            if (_cursorId != Base.CoreSprite.CursorSmall) // Inventory screen, check what's being held.
            {
                _itemSprite?.Dispose(); _itemSprite = null;
                _itemAmountSprite?.Dispose(); _itemAmountSprite = null;
                return;
            }

            int subItem = 0;
            ITexture texture = null;

            switch (held.Item)
            {
                case Gold: texture = assets.LoadTexture(Base.CoreSprite.UiGold); break;
                case Rations: texture = assets.LoadTexture(Base.CoreSprite.UiFood); break;
                case ItemData item:
                {
                    texture = assets.LoadTexture(item.Icon);
                    subItem = item.IconSubId + _frame % item.IconAnim;
                    break;
                }
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

                var key = new SpriteKey(texture, SpriteSampler.Point, DrawLayer.Cursor, SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);
                _itemSprite = factory.CreateSprites(key, 1, this);
            }

            var subImage = texture.Regions[subItem];

            bool lockWasTaken = false;
            var instances = _itemSprite.Lock(ref lockWasTaken);
            try
            {
                // TODO: Quantity text
                instances[0] = new SpriteInstanceData(
                    normPosition + new Vector3(window.UiToNormRelative(6, 6), 0),
                    window.UiToNormRelative(subImage.Size),
                    subImage, SpriteFlags.TopMid);
            }
            finally { _itemSprite.Unlock(lockWasTaken); }

            if (_itemAmountSprite != null)
                _itemAmountSprite.Position = normPosition + new Vector3(window.UiToNormRelative(6, 18), 0);
        }

        string GetAmountText()
        {
            var hand = Resolve<IInventoryManager>().ItemInHand;
            var amount = hand.Amount;
            if (amount < 2)
                return null;

            return
                hand.Item is Gold
                ? $"{amount / 10}.{amount % 10}"
                : amount.ToString(CultureInfo.InvariantCulture); // i18n: Will need updating if we want separators or non-Hindu-Arabic numerals.
        }
    }
}
