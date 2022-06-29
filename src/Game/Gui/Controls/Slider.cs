using System;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui.Controls;

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

    public Slider(Func<int> getter, Action<int> setter, int min, int max, Func<int, string> format = null)
    {
        _getter = getter;
        _setter = setter;
        _min = min;
        _max = max;

        _decrement = new Button("<") { Typematic = true }.OnClick(Decrement);
        _increment = new Button(">") { Typematic = true }.OnClick(Increment);

        var track = new SliderTrack(getter, x => _setter(Math.Clamp(x, _min, _max)), min, max, format);

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
        foreach (var child in Children)
        {
            if (child is not IUiElement { IsActive: true } childElement)
                continue;

            var childSize = childElement.GetSize();
            size.X += childSize.X;

            if (childSize.Y > size.Y)
                size.Y = childSize.Y;
        }
        return size;
    }

    protected override int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        int maxOrder = order;
        var decWidth = (int)_decrement.GetSize().X;
        var incWidth = (int)_increment.GetSize().X;

        maxOrder = Math.Max(maxOrder, func(_decrement,
            new Rectangle(extents.X, extents.Y, decWidth, extents.Height),
            order + 1, context));

        maxOrder = Math.Max(maxOrder, func(_frame, new Rectangle(
            extents.X + decWidth,
            extents.Y,
            extents.Width - decWidth - incWidth,
            extents.Height
        ), order + 1, context));

        maxOrder = Math.Max(maxOrder, func(_increment,
            new Rectangle(extents.X + extents.Width - incWidth, extents.Y, incWidth, extents.Height),
            order + 1, context));

        return maxOrder;
    }
}