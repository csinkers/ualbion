using System;
using UAlbion.Config;
using UAlbion.Core.Events;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Status
{
    public class ClockWidget : Dialog
    {
        static readonly (int, int) Position  = (5, 74);
        static readonly (int, int) Size  = (31, 25);
        static readonly (int, int) DigitSize  = (5, 7);
        static readonly (int, int)[] Digits =
        {
            ( 3, 10),
            ( 9, 10),
            (17, 10),
            (23, 10)
        };

        readonly UiSpriteElement[] _digits;
        DateTime _lastTime;

        public ClockWidget() : base(DialogPositioning.TopLeft)
        {
            On<MinuteElapsedEvent>(_ => Update());
            On<ActivateItemEvent>(e =>
            {
                if (e.Item == Base.Item.Clock) // Do a one shot update next time the scene is being drawn.
                    On<RenderEvent>(_ => Update());
            });

            var face = new UiSpriteElement(Base.CoreSprite.Clock);
            _digits = new[]
            {
                new UiSpriteElement(AssetId.None),
                new UiSpriteElement(AssetId.None),
                new UiSpriteElement(AssetId.None),
                new UiSpriteElement(AssetId.None)
            };

            var digitStack = new FixedPositionStack()
                .Add(_digits[0], Digits[0].Item1, Digits[0].Item2, DigitSize.Item1, DigitSize.Item2)
                .Add(_digits[1], Digits[1].Item1, Digits[1].Item2, DigitSize.Item1, DigitSize.Item2)
                .Add(_digits[2], Digits[2].Item1, Digits[2].Item2, DigitSize.Item1, DigitSize.Item2)
                .Add(_digits[3], Digits[3].Item1, Digits[3].Item2, DigitSize.Item1, DigitSize.Item2);

            var layerStack = new LayerStack(face, digitStack);
            AttachChild(new FixedPositionStack().Add(layerStack, Position.Item1, Position.Item2, Size.Item1, Size.Item2));
        }

        protected override void Subscribed()
        {
            Update();
            base.Subscribed();
        }

        void Update()
        {
            bool active = ((Resolve<IGameState>()?.ActiveItems ?? 0) & ActiveItems.Clock) != 0;
            if (active)
            {
                var time = Resolve<IGameState>().Time;
                if (_lastTime == DateTime.MinValue || _lastTime.Minute != time.Minute)
                {
                    _lastTime = time;
                    _digits[0].Id = DigitToSprite(time.Hour / 10);
                    _digits[1].Id = DigitToSprite(time.Hour % 10);
                    _digits[2].Id = DigitToSprite(time.Minute / 10);
                    _digits[3].Id = DigitToSprite(time.Minute % 10);
                }
            }

            foreach (var child in Children)
                child.IsActive = active;

            Off<RenderEvent>();
        }

        static SpriteId DigitToSprite(int n) => n switch
            {
                0 => Base.CoreSprite.ClockNum0,
                1 => Base.CoreSprite.ClockNum1,
                2 => Base.CoreSprite.ClockNum2,
                3 => Base.CoreSprite.ClockNum3,
                4 => Base.CoreSprite.ClockNum4,
                5 => Base.CoreSprite.ClockNum5,
                6 => Base.CoreSprite.ClockNum6,
                7 => Base.CoreSprite.ClockNum7,
                8 => Base.CoreSprite.ClockNum8,
                _ => Base.CoreSprite.ClockNum9
            };
    }
}
