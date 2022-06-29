using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Controls;

public class ProgressBar : UiElement
{
    readonly ButtonFrame _frame;
    readonly UiRectangle _bar;
    readonly IText _hover;
    readonly Func<int> _getValue;
    readonly Func<int> _getMax;
    readonly int _absoluteMax;
    Rectangle _lastExtents;
    int _lastValue;
    int _lastMax;

    public ProgressBar(IText hover, Func<int> getValue, Func<int> getMax, int absoluteMax)
    {
        On<HoverEvent>(_ => Hover());
        On<BlurEvent>(_ => Raise(new HoverTextEvent(null)));

        _hover = hover;
        _getValue = getValue;
        _getMax = getMax;
        _absoluteMax = absoluteMax;

        _bar = new UiRectangle(CommonColor.Blue4);
        _frame = AttachChild(new ButtonFrame(_bar) { State = ButtonState.Pressed, Padding = 0 });
    }

    void Hover()
    {
        if (_hover == null)
            return;

        Raise(new HoverTextEvent(_hover));
    }

    void Update(Rectangle extents)
    {
        var value = _getValue();
        var max = _getMax();

        if (_lastExtents == extents && _lastValue == value && _lastMax == max)
            return;

        _lastExtents = extents;
        _lastValue = value;
        _lastMax = max;

        bool isSuperCharged = value > max;
        if (isSuperCharged) value = max;
        if (max == 0) max = 1;

        _bar.Color = isSuperCharged ? CommonColor.Yellow4 : CommonColor.Blue4;
        _bar.MeasureSize = new Vector2((extents.Width - 3) * (float)max / _absoluteMax, 6);
        _bar.DrawSize = new Vector2(_bar.MeasureSize.X * value / max, _bar.MeasureSize.Y);
    }

    protected override int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        Update(extents);
        var frameExtents = new Rectangle(extents.X, extents.Y, (int)_frame.GetSize().X, extents.Height);
        return base.DoLayout(frameExtents, order, context, func);
    }
}