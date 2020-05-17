using System;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Controls
{
    public class SliderThumb : UiElement
    {
        readonly TextElement _text;
        readonly ButtonFrame _frame;
        readonly Func<int> _getter;
        int _lastValue = int.MaxValue;

        public SliderThumb(Func<int> getter)
        {
            On<HoverEvent>(e => _frame.State = ButtonState.Hover);
            On<BlurEvent>(e => _frame.State = ButtonState.Normal);

            _getter = getter;
            _text = new TextElement("").Center();
            _frame = new ButtonFrame(_text) { Theme = ButtonTheme.SliderThumb };
            AttachChild(_frame);
        }

        public ButtonState State { get => _frame.State; set => _frame.State = value; }
        protected override void Subscribed() => Rebuild();

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
