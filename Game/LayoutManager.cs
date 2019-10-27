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
        void Add(IUiElement topLevelElement, DialogPositioning positioning);
        void Remove(IUiElement topLevelElement);
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

        readonly IDictionary<IUiElement, DialogPositioning> _elements = new Dictionary<IUiElement, DialogPositioning>(); // Top-level elements
        IReadOnlyList<IUiElement> _lastSelection = new List<IUiElement>();

        void DoLayout(Action<Rectangle, IUiElement> action)
        {
            int uiWidth = UiConstants.ActiveAreaExtents.Width;
            int uiHeight = UiConstants.ActiveAreaExtents.Height;
            foreach (var (element, positioning) in _elements)
            {
                var size = element.GetSize();

                int x; int y;
                switch(positioning)
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

                action(new Rectangle(x, y, (int)size.X, (int)size.Y), element);
            }
        }

        void Render(RenderEvent renderEvent)
        {
            DoLayout((extents, element) => element.Render(extents, (int)DrawLayer.Interface, renderEvent.Add));
        }

        void Select(ScreenCoordinateSelectEvent selectEvent)
        {
            var window = Resolve<IWindowManager>();
            var normPosition = window.PixelToNorm(selectEvent.Position);
            var uiPosition = window.NormToUi(normPosition);

            var newSelection = new List<IUiElement>();
            DoLayout((extents, element) =>
                {
                    element.Select(uiPosition, extents, (int)DrawLayer.Interface, 
                        (order, target) =>
                        {
                            float z = 1.0f - order / (float)DrawLayer.MaxLayer;
                            var intersectionPoint = new Vector3(normPosition, z);
                            selectEvent.RegisterHit(z, new Selection(intersectionPoint, target));
                            if(target is IUiElement subElement)
                                newSelection.Add(subElement);
                        }
                    );
                });

            var focused = newSelection.Except(_lastSelection);
            var blurred = _lastSelection.Except(newSelection);
            _lastSelection = newSelection;

            Raise(new UiSelectedEvent(newSelection, focused, blurred));
        }

        public LayoutManager() : base(Handlers) { }
        public void Add(IUiElement topLevelElement, DialogPositioning positioning)
        {
            _elements[topLevelElement] = positioning;
        }

        public void Remove(IUiElement topLevelElement)
        {
            _elements.Remove(topLevelElement);
        }
    }

    public class UiSelectedEvent : GameEvent, IVerboseEvent
    {
        public IReadOnlyList<IUiElement> SelectedItems { get; }
        public IReadOnlyList<IUiElement> FocusedItems { get; }
        public IReadOnlyList<IUiElement> BlurredItems { get; }

        public UiSelectedEvent(IEnumerable<IUiElement> selectedItems, IEnumerable<IUiElement> focused, IEnumerable<IUiElement> blurred)
        {
            SelectedItems = new List<IUiElement>(selectedItems);
            FocusedItems = new List<IUiElement>(focused);
            BlurredItems = new List<IUiElement>(blurred);
        }
    }
}
