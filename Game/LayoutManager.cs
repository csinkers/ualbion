using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using Veldrid;

namespace UAlbion.Game
{
    public interface ILayoutManager
    {
    }

    public enum DialogPositioning
    {
        Center,
        Bottom,
        Top,
        Left,
        Right,
        BottomLeft,
        TopLeft,
        TopRight,
        BottomRight,
        StatusBar,
    }

    public class LayoutManager : Component, ILayoutManager
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<LayoutManager, RenderEvent>((x,e) => x.Render(e)), 
            H<LayoutManager, ScreenCoordinateSelectEvent>((x,e) => x.Select(e))
        );

        IReadOnlyList<IUiElement> _lastSelection = new List<IUiElement>();

        void DoLayout(Func<Rectangle, int, IUiElement, int> action)
        {
            int order = (int)DrawLayer.Interface;
            int uiWidth = UiConstants.ActiveAreaExtents.Width;
            int uiHeight = UiConstants.ActiveAreaExtents.Height;
            var dialogs = new List<IDialog>();
            Raise(new CollectDialogsEvent(dialogs.Add));
            foreach (var dialog in dialogs.OrderBy(x => x.Depth))
            {
                var size = dialog.GetSize();

                int x; int y;
                switch(dialog.Positioning)
                {
                    case DialogPositioning.Center:
                        x = (uiWidth - (int)size.X) / 2;
                        y = (uiHeight - (int)size.Y) / 2;
                        break;
                    case DialogPositioning.Bottom:
                        x = (uiWidth - (int)size.X) / 2;
                        y = uiHeight - (int)size.Y;
                        break;
                    case DialogPositioning.Top:
                        x = (uiWidth - (int)size.X) / 2;
                        y = 0;
                        break;
                    case DialogPositioning.Left:
                        x = 0;
                        y = (uiHeight - (int)size.Y) / 2;
                        break;
                    case DialogPositioning.Right:
                        x = uiWidth - (int)size.X;
                        y = (uiHeight - (int)size.Y) / 2;
                        break;
                    case DialogPositioning.BottomLeft:
                        x = 0;
                        y = uiHeight - (int)size.Y;
                        break;
                    case DialogPositioning.TopLeft:
                        x = 0; 
                        y = 0;
                        break;
                    case DialogPositioning.TopRight:
                        x = uiWidth - (int)size.X;
                        y = 0;
                        break;
                    case DialogPositioning.BottomRight:
                        x = uiWidth - (int)size.X;
                        y = uiHeight - (int)size.Y;
                        break;
                    case DialogPositioning.StatusBar:
                        x = (uiWidth - (int)size.X) / 2;
                        y = UiConstants.UiExtents.Height - (int)size.Y;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                order = action(new Rectangle(x, y, (int)size.X, (int)size.Y), order, dialog);
            }
        }

        void Render(RenderEvent renderEvent)
        {
            DoLayout((extents, order, element) => element.Render(extents, order, renderEvent.Add));
        }

        void Select(ScreenCoordinateSelectEvent selectEvent)
        {
            var window = Resolve<IWindowManager>();
            var normPosition = window.PixelToNorm(selectEvent.Position);
            var uiPosition = window.NormToUi(normPosition);

            var newSelection = new List<IUiElement>();
            DoLayout((extents, dialogOrder, element) =>
                    element.Select(uiPosition, extents, dialogOrder, (order, target) =>
                        {
                            float z = 1.0f - order / (float)DrawLayer.MaxLayer;
                            var intersectionPoint = new Vector3(normPosition, z);
                            selectEvent.RegisterHit(z, new Selection(intersectionPoint, target));
                            if(target is IUiElement subElement)
                                newSelection.Add(subElement);
                        }));

            var focused = newSelection.Except(_lastSelection);
            var blurred = _lastSelection.Except(newSelection);
            _lastSelection = newSelection;

            Raise(new UiSelectedEvent(newSelection, focused, blurred));
        }

        public LayoutManager() : base(Handlers) { }
    }
}
