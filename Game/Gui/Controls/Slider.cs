﻿using System;
using System.Linq;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui.Controls
{
    public class Slider : UiElement
    {
        readonly Func<int> _getter;
        readonly Action<int> _setter;

        readonly Button _decrement;
        readonly ButtonFrame _frame;
        readonly Button _increment;

        readonly int _min;
        readonly int _max;

        void Decrement() => Adjust(-1);
        void Increment() => Adjust(1);

        void Adjust(int amount)
        {
            var value = _getter();
            value = Math.Clamp(value + amount, _min, _max);
            _setter(value);
        }

        public Slider(Func<int> getter, Action<int> setter, int min, int max)
        {
            _getter = getter;
            _setter = setter;
            _min = min;
            _max = max;

            _decrement = new Button("<", Decrement) { Typematic = true };
            _increment = new Button(">", Increment) { Typematic = true };

            var track = new SliderTrack(getter, x =>
            {
                if (x >= _min && x <= _max)
                    _setter(x);
            }, min, max);

            _frame = new ButtonFrame(track)
            {
                State = ButtonState.Pressed,
                Padding = 0
            };

            AttachChild(_decrement);
            AttachChild(_frame);
            AttachChild(_increment);
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

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            int maxOrder = order;
            var decWidth = (int)_decrement.GetSize().X;
            var incWidth = (int)_increment.GetSize().X;

            maxOrder = Math.Max(maxOrder, func(_decrement,
                new Rectangle(extents.X, extents.Y, decWidth, extents.Height),
                order + 1));

            maxOrder = Math.Max(maxOrder, func(_frame, new Rectangle(
                    extents.X + decWidth,
                    extents.Y,
                extents.Width - decWidth - incWidth,
                    extents.Height
                ), order + 1));

            maxOrder = Math.Max(maxOrder, func(_increment,
                new Rectangle(extents.X + extents.Width - incWidth, extents.Y, incWidth, extents.Height),
                order + 1));

            return maxOrder;
        }
    }
}
