using System;
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
        const float UiScaleFactor = 4.0f; // TODO: Config
        readonly Assets _assets;
        CoreSpriteId _cursorId = CoreSpriteId.Cursor;
        Vector2 _position;
        Vector2 _size;
        Vector2 _windowSize;
        public CursorManager(Assets assets) : base(Handlers)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        static readonly Handler[] Handlers = {
            new Handler<CursorManager, InputEvent>((x,e) => x.Input(e)),
            new Handler<CursorManager, RenderEvent>((x,e) => x.Render(e)),
            new Handler<CursorManager, SetCursorEvent>((x,e) => x.SetCursor(e.CursorId)),
            new Handler<CursorManager, WindowResizedEvent>((x, e) => x._windowSize = new Vector2(e.Width, e.Height))
        };

        void SetCursor(CoreSpriteId id)
        {
            _cursorId = id;
            var texture = _assets.LoadTexture(_cursorId);
            _size = new Vector2(texture.Width, texture.Height);
        }

        void Input(InputEvent e)
        {
            _position = e.Snapshot.MousePosition;
        }

        void Render(RenderEvent e)
        {
            if (_windowSize.X < 1 || _windowSize.Y < 1)
                return;
            var drawLayer = DrawLayer.Interface;
            var position = new Vector3(
                2 * _position.X / _windowSize.X - 1.0f,
                1.0f - 2 * _position.Y / _windowSize.Y,
                drawLayer.ToZCoordinate(0));

            var size = new Vector2(
                UiScaleFactor * _size.X / _windowSize.X,
                -UiScaleFactor * _size.Y / _windowSize.Y);

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