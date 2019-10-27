using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class SliderTrack : UiElement
    {
        readonly string _id;
        readonly SliderThumb _thumb;
        readonly Func<int> _getter;
        readonly int _min;
        readonly int _max;

        float _clickPosition;
        SliderState _state;

        enum SliderState
        {
            Normal, // The default state before the user has interacted with the slider.
            Clicked, // Set by the click event then handled by select on the next frame.
            ClickHandled, // If the click was to the left or right of the thumb, then we transition to this state after handling it.
            DraggingThumb, // If the click was on the thumb, then we transition to this state.
        }

        static readonly HandlerSet Handlers = new HandlerSet(
            H<SliderTrack, UiLeftClickEvent>((x, e) =>
            {
                x._thumb.State = ButtonState.Clicked;
                x._state = SliderState.Clicked;
                e.Propagating = false;
                x.Raise(new SetExclusiveMouseModeEvent(x));
            }),
            H<SliderTrack, UiLeftReleaseEvent>((x, _) =>
            {
                x._state = SliderState.Normal;
                x._thumb.State = ButtonState.Normal;
            })
        );

        public SliderTrack(string id, Func<int> getter, int min, int max) : base(Handlers)
        {
            _id = id;
            _getter = getter;
            _min = min;
            _max = max;
            _thumb = new SliderThumb(getter);
            Children.Add(_thumb);
        }

        public override Vector2 GetSize() => _thumb.GetSize();

        Rectangle ThumbExtents(Rectangle extents)
        {
            var size = _thumb.GetSize();
            size.X = (int)Math.Max(size.X, 8 * 3); // Reserve at least enough space for 3 digits.
            size.X = (int)Math.Max(size.X, (float)extents.Width / (_max - _min));
            int spareWidth = extents.Width - (int)size.X;
            int currentValue = _getter();

            int position = extents.X + (int)(spareWidth * (float)(currentValue - _min) / (_max - _min));
            return new Rectangle(position, extents.Y, (int)size.X, extents.Height);
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            return _thumb.Render(ThumbExtents(extents), order, addFunc);
        }

        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return order;

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
                        Raise(new SliderMovedEvent(_id, value));
                        _state = SliderState.ClickHandled;
                    }
                    else
                    {
                        value += Math.Max(1, (_max - _min) / 10);
                        Raise(new SliderMovedEvent(_id, value));
                        _state = SliderState.ClickHandled;
                    }
                    break;

                case SliderState.DraggingThumb:
                    float equivalentThumbPosition = uiPosition.X - _clickPosition;
                    int spareWidth = extents.Width - thumbExtents.Width;
                    int newValue = (int)((_max - _min)*(equivalentThumbPosition - extents.X) / spareWidth) + _min;
                    if (newValue != value)
                        Raise(new SliderMovedEvent(_id, newValue));
                    break;
            }

            var maxOrder = _thumb.Select(uiPosition, thumbExtents, order + 1, registerHitFunc);
            registerHitFunc(order, this);
            return maxOrder;
        }
    }
}