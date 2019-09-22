using System;
using System.Diagnostics;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class CursorManager : Component
    {
        const float UiScaleFactor = 4.0f; // TODO: Use config / heuristic
        readonly Assets _assets;
        CoreSpriteId _cursorId = CoreSpriteId.Cursor;
        Vector2 _position;
        Vector2 _hotspot;
        Vector2 _size;

        public CursorManager(Assets assets) : base(Handlers)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        static readonly Handler[] Handlers = {
            new Handler<CursorManager, InputEvent>((x,e) => x._position = e.Snapshot.MousePosition - x._hotspot),
            new Handler<CursorManager, RenderEvent>((x,e) => x.Render(e)),
            new Handler<CursorManager, SetCursorEvent>((x,e) => x.SetCursor(e.CursorId)),
            new Handler<CursorManager, SetCursorPositionEvent>((x,e) => x._position = new Vector2(e.X, e.Y) - x._hotspot),
        };

        void SetCursor(CoreSpriteId id)
        {
            var texture = _assets.LoadTexture(id);
            var config = _assets.LoadCoreSpriteInfo(id);
            _cursorId = id;
            _size = new Vector2(texture.Width, texture.Height);
            _hotspot = new Vector2(config.Hotspot.X, config.Hotspot.Y);
        }

        void Render(RenderEvent e)
        {
            var windowSize = Exchange.Resolve<IWindowState>().Size;
            if (windowSize.X < 1 || windowSize.Y < 1)
                return;

            var drawLayer = DrawLayer.Interface;
            //if (((_position + _hotspot) - windowSize / 2).LengthSquared() > 1)
            //    Debugger.Break();

            var position = new Vector3(
                2 * _position.X / windowSize.X - 1.0f,
                1.0f - 2 * _position.Y / windowSize.Y,
                0.0f);

            var size = new Vector2(UiScaleFactor, -UiScaleFactor) * _size / windowSize;

            e.Add(new SpriteDefinition<CoreSpriteId>(_cursorId,
                0,
                position,
                (int)drawLayer,
                false,
                SpriteFlags.NoTransform,
                size));
        }
    }
}