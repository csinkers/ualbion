using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui.Controls;

public class SliderTrack : UiElement
{
    readonly SliderThumb _thumb;
    readonly Func<int> _getter;
    readonly Action<int> _setter;
    readonly int _min;
    readonly int _max;

    Rectangle _lastExtents;
    float _clickPosition;
    SliderState _state;

    enum SliderState
    {
        Normal, // The default state before the user has interacted with the slider.
        Clicked, // Set by the click event then handled by select on the next frame.
        ClickHandled, // If the click was to the left or right of the thumb, then we transition to this state after handling it.
        DraggingThumb, // If the click was on the thumb, then we transition to this state.
    }

    public SliderTrack(Func<int> getter, Action<int> setter, int min, int max, Func<int, string> format = null)
    {
        On<UiLeftClickEvent>(e =>
        {
            _thumb.State = ButtonState.Clicked;
            _state = SliderState.Clicked;
            e.Propagating = false;
            // Raise(new SetExclusiveMouseModeEvent(this));
        });
        On<UiLeftReleaseEvent>(e =>
        {
            _state = SliderState.Normal;
            _thumb.State = ButtonState.Normal;
        });
        On<UiMouseMoveEvent>(e =>
        {
            if (_state != SliderState.DraggingThumb)
                return;

            var thumbExtents = ThumbExtents(_lastExtents);
            float equivalentThumbPosition = e.X - _clickPosition;
            int spareWidth = _lastExtents.Width - thumbExtents.Width;
            int newValue = (int)((_max - _min)*(equivalentThumbPosition - _lastExtents.X) / spareWidth) + _min;
            if (newValue != _getter())
                _setter(newValue);
        });

        _getter = getter;
        _setter = setter;
        _min = min;
        _max = max;
        _thumb = new SliderThumb(getter, format);
        AttachChild(_thumb);
    }

    public override Vector2 GetSize() => _thumb.GetSize();

    Rectangle ThumbExtents(Rectangle extents)
    {
        var size = _thumb.GetSize();
        size.X = (int)Math.Max(size.X, 8 * 3); // Reserve at least enough space for 3 digits.
        size.X = (int)Math.Max(size.X, (float)extents.Width / (_max + 1 - _min));
        int spareWidth = extents.Width - (int)size.X;
        int currentValue = _getter();

        int position = extents.X + (int)(spareWidth * (float)(currentValue - _min) / (_max - _min));
        return new Rectangle(position, extents.Y, (int)size.X, extents.Height);
    }

    public override int Render(Rectangle extents, int order) => _thumb.Render(ThumbExtents(extents), order);

    public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
    {
        if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
            return order;

        _lastExtents = extents;
        var thumbExtents = ThumbExtents(extents);
        int value = _getter();
        switch (_state)
        {
            case SliderState.Normal:
            case SliderState.ClickHandled: break; // Nothing to be done
            case SliderState.Clicked:
                if (thumbExtents.Contains((int) uiPosition.X, (int) uiPosition.Y))
                {
                    // Not sure why the -1 is necessary, can't be bothered investigating further as it works well enough.
                    _clickPosition = uiPosition.X - thumbExtents.X - 1;
                    _state = SliderState.DraggingThumb;
                }
                else if (uiPosition.X < thumbExtents.X)
                {
                    value -= Math.Max(1, (_max - _min) / 10);
                    _setter(value);
                    _state = SliderState.ClickHandled;
                }
                else
                {
                    value += Math.Max(1, (_max - _min) / 10);
                    _setter(value);
                    _state = SliderState.ClickHandled;
                }
                break;

            case SliderState.DraggingThumb:
                break;
        }

        var maxOrder = _thumb.Select(uiPosition, thumbExtents, order + 1, registerHitFunc);
        registerHitFunc(order, this);
        return maxOrder;
    }
}