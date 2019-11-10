using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class CursorManager : Component
    {
        CoreSpriteId _cursorId = CoreSpriteId.Cursor;
        Vector2 _position;
        Vector2 _hotspot;
        Vector2 _size;
        float _special;
        float _special2;

        public CursorManager() : base(Handlers) { }

        static readonly HandlerSet Handlers = new HandlerSet(
            H<CursorManager, InputEvent>((x,e) => x._position = e.Snapshot.MousePosition - x._hotspot),
            H<CursorManager, RenderEvent>((x,e) => x.Render(e)),
            H<CursorManager, SetCursorEvent>((x,e) => x.SetCursor(e.CursorId)),
            H<CursorManager, SetCursorPositionEvent>((x,e) => x._position = new Vector2(e.X, e.Y) - x._hotspot),
            H<CursorManager, WindowResizedEvent>((x,e) => x.SetCursor(x._cursorId)),
            H<CursorManager, SpecialEvent>((x, e) => { x._special += (float)e.Argument / 4; x.SetCursor(x._cursorId); }),
            H<CursorManager, Special2Event>((x, e) => { x._special2 += (float)e.Argument / 4; x.SetCursor(x._cursorId); })
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
            _hotspot += window.GuiScale * new Vector2(_special, _special2);
        }

        void Render(RenderEvent e)
        {
            var window = Resolve<IWindowManager>();
            if (window.Size.X < 1 || window.Size.Y < 1)
                return;

            var drawLayer = DrawLayer.MaxLayer;
            //if (((_position + _hotspot) - windowSize / 2).LengthSquared() > 1)
            //    Debugger.Break();

            var position = new Vector3(
                window.PixelToNorm(_position),
                0.0f);

            var size = new Vector2(window.GuiScale, -window.GuiScale) * _size / window.Size;

            e.Add(new Sprite<CoreSpriteId>(_cursorId,
                0,
                position,
                (int)drawLayer,
                SpriteFlags.NoTransform | SpriteFlags.NoDepthTest,
                size));
        }
    }
}