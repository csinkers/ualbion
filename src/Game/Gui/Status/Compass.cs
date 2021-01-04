using System;
using System.Numerics;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Status
{
    public class Compass : Dialog
    {
        const int TicksPerFrame = 6;
        const int FrameCount = 8;
        const int MarkerRadius = 10;
        static readonly (int, int) Position  = (7, 5);
        static readonly (int, int) Size  = (29, 29);
        static readonly Vector2 CenterRelative = new Vector2(15, 15);
        static readonly (int, int) MarkerSize = (6, 6);

        readonly UiSpriteElement _face;
        readonly UiSpriteElement _marker;
        readonly FixedPositionStack _markerStack;
        readonly FixedPositionStack _mainStack;
        byte _ticks;
        byte _frame;

        public Compass() : base(DialogPositioning.TopLeft)

        {
            On<FastClockEvent>(_ => Update());
            _face = new UiSpriteElement(AssetId.None);
            _marker = new UiSpriteElement(AssetId.None);
            _markerStack = new FixedPositionStack().Add(_marker, 0, 0);
            var layerStack = new LayerStack(_face, _markerStack);
            _mainStack = AttachChild(new FixedPositionStack()
                .Add(layerStack, Position.Item1, Position.Item2, Size.Item1, Size.Item2));
        }

        void Update()
        {
            bool active = ((Resolve<IGameState>()?.ActiveItems ?? 0) & ActiveItems.Compass) != 0;
            if (active)
            {
                _ticks++;
                if (_ticks == TicksPerFrame)
                {
                    _frame++;
                    _ticks = 0;
                    if (_frame == FrameCount)
                        _frame = 0;
                }

                var language = Resolve<ISettings>()?.Gameplay.Language ?? GameLanguage.English;
                _face.Id = language switch
                    {
                        GameLanguage.German => Base.CoreSprite.CompassDe,
                        GameLanguage.French => Base.CoreSprite.CompassFr,
                        _ => Base.CoreSprite.CompassEn
                    };

                _marker.Id = (_frame & 0x7) switch
                {
                    0 => Base.CoreSprite.CompassDot0,
                    1 => Base.CoreSprite.CompassDot1,
                    2 => Base.CoreSprite.CompassDot2,
                    3 => Base.CoreSprite.CompassDot3,
                    4 => Base.CoreSprite.CompassDot4,
                    5 => Base.CoreSprite.CompassDot5,
                    6 => Base.CoreSprite.CompassDot6,
                    _ => Base.CoreSprite.CompassDot7
                };

                var (mx, my) = CalculateMarkerPosition();
                _markerStack.Move(_marker, mx, my);
            }

            _mainStack.IsActive = active;
        }

        (int,int) CalculateMarkerPosition()
        {
            var camera = Resolve<ICamera>() as PerspectiveCamera;
            var angle = camera?.Yaw ?? 0;
            angle += (float)Math.PI / 2;
            var direction = new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle));
            var pos = CenterRelative + MarkerRadius * direction;
            return ((int)pos.X - MarkerSize.Item1 / 2, (int)pos.Y - MarkerSize.Item2 / 2);
        }
    }
}