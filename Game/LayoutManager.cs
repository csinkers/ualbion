using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Events;
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
    }

    public class LayoutManager : Component, ILayoutManager
    {
        static readonly Handler[] Handlers =
        {
            new Handler<LayoutManager, RenderEvent>((x,e) => x.Render(e)), 
        };

        readonly IDictionary<IUiElement, DialogPositioning> _elements = new Dictionary<IUiElement, DialogPositioning>(); // Top-level elements

        void Render(RenderEvent renderEvent)
        {
            foreach (var (element, positioning) in _elements)
            {
                var size = element.GetSize();
                int x;
                int y;
                var window = Exchange.Resolve<IWindowState>();

                switch(positioning)
                {
                    case DialogPositioning.Center:
                        x = (window.UiWidth - (int)size.X) / 2;
                        y = (window.UiHeight - (int)size.Y) / 2;
                        break;
                    case DialogPositioning.Bottom:
                        x = (window.UiWidth - (int)size.X) / 2;
                        y = window.UiHeight - (int)size.Y;
                        break;
                    case DialogPositioning.Top:
                        x = (window.UiWidth - (int)size.X) / 2;
                        y = 0;
                        break;
                    case DialogPositioning.Left:
                        x = 0;
                        y = (window.UiHeight - (int)size.Y) / 2;
                        break;
                    case DialogPositioning.Right:
                        x = window.UiWidth - (int)size.X;
                        y = (window.UiHeight - (int)size.Y) / 2;
                        break;
                    case DialogPositioning.BottomLeft:
                        x = 0;
                        y = window.UiHeight - (int)size.Y;
                        break;
                    case DialogPositioning.TopLeft:
                        x = 0; 
                        y = 0;
                        break;
                    case DialogPositioning.TopRight:
                        x = window.UiWidth - (int)size.X;
                        y = 0;
                        break;
                    case DialogPositioning.BottomRight:
                        x = window.UiWidth - (int)size.X;
                        y = window.UiHeight - (int)size.Y;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                element.Render(new Rectangle(x, y, (int)size.X, (int)size.Y), renderEvent.Add);
            }
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
}
