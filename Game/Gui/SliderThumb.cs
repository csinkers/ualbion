using System;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui
{
    public class SliderThumb : UiElement
    {
        static readonly ButtonFrame.ITheme Theme = new SliderThumbTheme();
        readonly TextSection _text;
        readonly ButtonFrame _frame;
        readonly Func<int> _getter;
        int _lastValue = int.MaxValue;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<SliderThumb, HoverEvent>((x, _) => x._frame.State = ButtonState.Hover),
            H<SliderThumb, BlurEvent>((x, _) => x._frame.State = ButtonState.Normal)
        );

        public SliderThumb(Func<int> getter) : base(Handlers)
        {
            _getter = getter;
            _text = new TextSection("").Center();
            _frame = new ButtonFrame(_text) { Theme = Theme };
            AttachChild(_frame);
        }

        public ButtonState State { get => _frame.State; set => _frame.State = value; }
        public override void Subscribed() => Rebuild();

        void Rebuild()
        {
            int currentValue = _getter();
            if (_lastValue != currentValue)
            {
                _text.LiteralString(currentValue.ToString());
                _lastValue = currentValue;
            }
        }

        public override int Render(Rectangle extents, int order)
        {
            Rebuild();
            return base.Render(extents, order);
        }
    }
}
