using System;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class ProgressBar : UiElement
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<ProgressBar, HoverEvent>((x, e) => x.Hover()),
            H<ProgressBar, BlurEvent>((x, e) => x.Raise(new HoverTextEvent("")))
        );

        readonly ButtonFrame _frame;
        readonly UiRectangle _bar;
        readonly IText _hover;
        readonly Func<int> _getValue;
        readonly Func<int> _getMax;
        readonly int _absoluteMax;
        Rectangle _lastExtents;
        int _lastValue;
        int _lastMax;

        public ProgressBar(IText hover, Func<int> getValue, Func<int> getMax, int absoluteMax) : base(Handlers)
        {
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

            var text = _hover.Get().FirstOrDefault() ?? new TextBlock();
            Raise(new HoverTextEvent(text.Text));
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

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            Update(extents);
            var frameExtents = new Rectangle(extents.X, extents.Y, (int)_frame.GetSize().X, extents.Height);
            return base.DoLayout(frameExtents, order, func);
        }
    }
}
