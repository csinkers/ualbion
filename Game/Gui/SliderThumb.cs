using System;
using UAlbion.Core;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class SliderThumb : UiElement
    {
        static readonly ButtonFrame.ITheme Theme = new SliderThumbTheme();
        readonly Text _text;
        readonly ButtonFrame _frame;
        readonly Func<int> _getter;
        int _lastValue = int.MaxValue;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<SliderThumb, UiHoverEvent>((x, _) => x._frame.State = ButtonState.Hover),
            H<SliderThumb, UiBlurEvent>((x, _) => x._frame.State = ButtonState.Normal)
        );

        public SliderThumb(Func<int> getter) : base(Handlers)
        {
            _getter = getter;
            _text = new Text("").Center();
            _frame = new ButtonFrame(_text) { Theme = Theme };
            Children.Add(_frame);
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

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            Rebuild();
            return base.Render(extents, order, addFunc);
        }
    }
}