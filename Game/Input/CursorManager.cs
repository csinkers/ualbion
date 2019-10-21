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

        public CursorManager() : base(Handlers) { }

        static readonly HandlerSet Handlers = new HandlerSet(
            H<CursorManager, InputEvent>((x,e) => x._position = e.Snapshot.MousePosition - x._hotspot),
            H<CursorManager, RenderEvent>((x,e) => x.Render(e)),
            H<CursorManager, SetCursorEvent>((x,e) => x.SetCursor(e.CursorId)),
            H<CursorManager, SetCursorPositionEvent>((x,e) => x._position = new Vector2(e.X, e.Y) - x._hotspot)
        );

        void SetCursor(CoreSpriteId id)
        {
            var assets = Resolve<IAssetManager>();
            var window = Resolve<IWindowManager>();
            var texture = assets.LoadTexture(id);
            var config = assets.LoadCoreSpriteInfo(id);
            _cursorId = id;
            _size = new Vector2(texture.Width, texture.Height);
            _hotspot = window.GuiScale * new Vector2(config.Hotspot.X, config.Hotspot.Y);
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

            e.Add(new SpriteDefinition<CoreSpriteId>(_cursorId,
                0,
                position,
                (int)drawLayer,
                SpriteFlags.NoTransform | SpriteFlags.NoDepthTest,
                size));
        }
    }
}