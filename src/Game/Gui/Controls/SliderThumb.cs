﻿using System;
using System.Globalization;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Controls;

public class SliderThumb : UiElement
{
    readonly SimpleText _text;
    readonly ButtonFrame _frame;
    readonly Func<int> _getter;
    readonly Func<int, string> _format;
    int _lastValue = int.MaxValue;

    public SliderThumb(Func<int> getter, Func<int, string> format = null)
    {
        _getter = getter;
        _format = format;
        _text = new SimpleText("").Center();
        _frame = new ButtonFrame(_text) { Theme = ButtonTheme.SliderThumb };
        AttachChild(_frame);

        On<HoverEvent>(_ => _frame.State = ButtonState.Hover);
        On<BlurEvent>(_ => _frame.State = ButtonState.Normal);
    }

    public ButtonState State { get => _frame.State; set => _frame.State = value; }
    protected override void Subscribed() => Rebuild();

    void Rebuild()
    {
        int currentValue = _getter();
        if (_lastValue == currentValue) 
            return;

        _lastValue = currentValue;
        _text.Text = _format == null
            ? currentValue.ToString(CultureInfo.InvariantCulture) // todo: i18n
            : _format(currentValue);
    }

    public override int Render(Rectangle extents, int order, LayoutNode parent)
    {
        Rebuild();
        return base.Render(extents, order, parent);
    }
}
