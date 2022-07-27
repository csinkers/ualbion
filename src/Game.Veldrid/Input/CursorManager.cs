using System.Globalization;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Input;
using UAlbion.Game.Settings;
using UAlbion.Game.Text;

namespace UAlbion.Game.Veldrid.Input;

public class CursorManager : ServiceComponent<ICursorManager>, ICursorManager
{
    static readonly Vector2 ItemSpriteOffset = new(3, 3);

    public Vector2 Position { get; private set; }
    Vector2 _hotspot;
    SpriteLease<SpriteInfo> _cursorSprite;
    SpriteLease<SpriteInfo> _itemSprite;
    SpriteLease<SpriteInfo> _hotspotSprite;
    PositionedSpriteBatch _itemAmountSprite;

    SpriteId _cursorId = Base.CoreGfx.Cursor;
    SpriteId _heldItemId = SpriteId.None;
    int _heldSubItem;
    int _heldItemFrames;
    int _heldItemCount;
    bool _heldItemCountUsesTenths;

    int _lastAmount;
    bool _dirty = true;
    bool _showCursor = true;
    bool _relative;
    int _frame;

    public CursorManager()
    {
        On<RenderEvent>(_ => Render());
        On<IdleClockEvent>(_ => _frame++);
        On<WindowResizedEvent>(_ => SetCursor(_cursorId));
        On<SetCursorEvent>(e => SetCursor(e.CursorId));
        On<ShowCursorEvent>(e => { _showCursor = e.Show; _dirty = true; });
        On<SetRelativeMouseModeEvent>(e => _relative = e.Enabled);
        On<SetHeldItemCursorEvent>(e =>
        {
            _heldItemId = e.Sprite;
            _heldSubItem = e.SubItem;
            _heldItemCount = e.ItemCount;
            _heldItemCountUsesTenths = e.UseTenths;
            _heldItemFrames = e.FrameCount < 1 ? 1 : e.FrameCount;
        });
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
        var hotspot = CursorHotspot.Parse(config?.Get<string>("Hotspot", null));
        _hotspot = hotspot == null
            ? Vector2.Zero
            : window.GuiScale * new Vector2(hotspot.X, hotspot.Y);
        _dirty = true;
    }

    void Render()
    {
        var showHotspot = (GetVar(UserVars.Debug.DebugFlags) & DebugFlags.ShowCursorHotspot) != 0;
        if (showHotspot != (_hotspotSprite == null))
            _dirty = true;

        if (!_dirty)
            return;
        _dirty = false;

        var assets = Resolve<IAssetManager>();
        var sm = Resolve<ISpriteManager<SpriteInfo>>();
        var window = Resolve<IWindowManager>();

        if (window.Size.X < 1 || window.Size.Y < 1)
            return;

        var position = new Vector3(window.PixelToNorm(Position - _hotspot), 1.0f);
        RenderCursor(assets, sm, window, position);
        RenderItemInHandCursor(assets, sm, window, position);
        RenderHotspot(sm, window, showHotspot);
    }

    void RenderHotspot(ISpriteManager<SpriteInfo> sm, IWindowManager window, bool showHotspot)
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
            _hotspotSprite = sm.Borrow(key, 1, this);
        }

        var position = new Vector3(window.PixelToNorm(Position), 0.0f);
        var size = window.UiToNormRelative(Vector2.One);

        bool lockWasTaken = false;
        var instances = _hotspotSprite.Lock(ref lockWasTaken);
        try
        {
            instances[0] = new SpriteInfo(SpriteFlags.TopLeft, position, size, commonColors.GetRegion(CommonColor.Yellow3));
        }
        finally { _hotspotSprite.Unlock(lockWasTaken); }
    }

    void RenderCursor(IAssetManager assets, ISpriteManager<SpriteInfo> sm, IWindowManager window, Vector3 position)
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
            _cursorSprite = sm.Borrow(key, 1, this);
        }

        var size = window.UiToNormRelative(new Vector2(cursorTexture.Width, cursorTexture.Height));

        bool lockWasTaken = false;
        var instances = _cursorSprite.Lock(ref lockWasTaken);
        try
        {
            instances[0] = new SpriteInfo(SpriteFlags.TopLeft, position, size, _cursorSprite.Key.Texture.Regions[0]);
        }
        finally { _cursorSprite.Unlock(lockWasTaken); }
    }

    void RenderItemInHandCursor(IAssetManager assets, ISpriteManager<SpriteInfo> sm, IWindowManager window, Vector3 normPosition)
    {
        if (_lastAmount != _heldItemCount)
        {
            var tm = Resolve<ITextManager>();
            _lastAmount = _heldItemCount;
            _itemAmountSprite?.Dispose();

            var itemAmountText = GetAmountText();
            _itemAmountSprite = itemAmountText == null
                ? null
                : tm.BuildRenderable(new TextBlock(itemAmountText), DrawLayer.MaxLayer, null, this);
        }

        if (_heldItemId.IsNone)
        {
            _itemSprite?.Dispose(); _itemSprite = null;
            _itemAmountSprite?.Dispose(); _itemAmountSprite = null;
            return;
        }

        if (_heldItemId.IsNone)
        {
            _itemSprite?.Dispose();
            _itemSprite = null;
            return;
        }

        ITexture texture = assets.LoadTexture(_heldItemId);
        if (texture == null)
        {
            _itemSprite?.Dispose();
            _itemSprite = null;
            return;
        }

        int subItem = _heldSubItem + _frame % _heldItemFrames;

        if (texture != _itemSprite?.Key.Texture)
        {
            _itemSprite?.Dispose();

            var key = new SpriteKey(texture, SpriteSampler.Point, DrawLayer.Cursor, SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);
            _itemSprite = sm.Borrow(key, 1, this);
        }

        var subImage = texture.Regions[subItem];

        bool lockWasTaken = false;
        var instances = _itemSprite.Lock(ref lockWasTaken);
        try
        {
            var normOffset = window.UiToNormRelative(ItemSpriteOffset.X, ItemSpriteOffset.Y);
            instances[0] = new SpriteInfo(
                SpriteFlags.TopLeft,
                normPosition + new Vector3(normOffset, 0),
                window.UiToNormRelative(subImage.Size), 
                subImage);
        }
        finally { _itemSprite.Unlock(lockWasTaken); }

        if (_itemAmountSprite != null)
            _itemAmountSprite.Position = normPosition + new Vector3(window.UiToNormRelative(subImage.Size), 0);
    }

    string GetAmountText()
    {
        if (_heldItemCount < 2)
            return null;

        return
            _heldItemCountUsesTenths
                ? $"{_heldItemCount / 10}.{_heldItemCount % 10}"
                : _heldItemCount.ToString(CultureInfo.InvariantCulture); // i18n: Will need updating if we want separators or non-Hindu-Arabic numerals.
    }
}
