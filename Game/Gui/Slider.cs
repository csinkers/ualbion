using System;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class Slider : UiElement
    {
        readonly string _decrementKey;
        readonly string _incrementKey;
        readonly string _id;

        readonly Func<int> _getter;
        readonly Action<int> _setter;

        readonly Button _decrement;
        readonly SliderTrack _track;
        readonly ButtonFrame _frame;
        readonly Button _increment;

        int _min = 0;
        int _max = 100;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<Slider, ButtonPressEvent>((x, e) =>
                {
                    if (e.ButtonId == x._decrementKey)
                        x.Adjust(-1);
                    else if (e.ButtonId == x._incrementKey)
                        x.Adjust(1);
                }),
            H<Slider, SliderMovedEvent>((x, e) =>
                {
                    if (e.SliderId == x._id && e.Position >= x._min && e.Position <= x._max)
                        x._setter(e.Position);
                })
            );


        void Adjust(int amount)
        {
            var value = _getter();
            value = Math.Clamp(value + amount, _min, _max);
            _setter(value);
        }

        public Slider(string id, Func<int> getter, Action<int> setter, int min, int max) : base(Handlers)
        {
            _getter = getter;
            _setter = setter;
            _min = min;
            _max = max;

            _decrementKey = $"{id}.Slider.Decrement";
            _incrementKey = $"{id}.Slider.Increment";
            _id = id;

            _decrement = new Button(_decrementKey, "<") { Typematic = true };
            _track = new SliderTrack(_id, getter, min, max);
            _increment = new Button(_incrementKey, ">") { Typematic = true };

            _frame = new ButtonFrame(_track)
            {
                State = ButtonState.Pressed,
                Padding = 0
            };

            Children.Add(_decrement);
            Children.Add(_frame);
            Children.Add(_increment);
        }

        public override Vector2 GetSize()
        {
            Vector2 size = Vector2.Zero;
            foreach (var child in Children.OfType<IUiElement>())
            {
                var childSize = child.GetSize();
                size.X += childSize.X;

                if (childSize.Y > size.Y)
                    size.Y = childSize.Y;
            }
            return size;
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            int maxOrder = order;
            var decWidth = (int)_decrement.GetSize().X;
            var incWidth = (int)_increment.GetSize().X;

            maxOrder = Math.Max(maxOrder, _decrement.Render(
                new Rectangle(extents.X, extents.Y, decWidth, extents.Height),
                order + 1, addFunc));

            maxOrder = Math.Max(maxOrder, _frame.Render(new Rectangle(
                    extents.X + decWidth,
                    extents.Y,
                extents.Width - decWidth - incWidth,
                    extents.Height
                ), order + 1, addFunc));

            maxOrder = Math.Max(maxOrder, _increment.Render(
                new Rectangle(extents.X + extents.Width - incWidth, extents.Y, incWidth, extents.Height),
                order + 1, addFunc));

            return maxOrder;
        }

        public override void Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return;

            var decWidth = (int)_decrement.GetSize().X;
            var incWidth = (int)_increment.GetSize().X;

             _decrement.Select(uiPosition,
                new Rectangle(extents.X, extents.Y, decWidth, extents.Height),
                order + 1, registerHitFunc);

             _frame.Select(uiPosition,
                 new Rectangle(
                    extents.X + decWidth,
                    extents.Y,
                extents.Width - decWidth - incWidth,
                    extents.Height
                ), order + 1, registerHitFunc);

             _increment.Select(uiPosition,
                new Rectangle(extents.X + extents.Width - incWidth, extents.Y, incWidth, extents.Height),
                order + 1, registerHitFunc);
        }
    }
}
