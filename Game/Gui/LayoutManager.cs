using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public interface ILayoutManager { }

    public class LayoutManager : Component, ILayoutManager
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<LayoutManager, RenderEvent>((x, _) => x.Render()),
            H<LayoutManager, ScreenCoordinateSelectEvent>((x, e) => x.Select(e))
        );

        public LayoutManager() : base(Handlers) { }

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

                void LayoutDialog(Vector2 dialogSize)
                {
                    var (x, y) = GetDialogPosition(dialog, dialogSize, uiWidth, uiHeight);
                    order = action(new Rectangle(x, y, (int)dialogSize.X, (int)dialogSize.Y), order, dialog);
                }
                LayoutDialog(size);

                var sizeAfter = dialog.GetSize(); // Hacky fix for first frame being different
                if(sizeAfter != size)
                    LayoutDialog(sizeAfter);
            }
        }

        void Render() => DoLayout((extents, order, element) => element.Render(extents, order));

        void Select(ScreenCoordinateSelectEvent selectEvent)
        {
            var window = Resolve<IWindowManager>();
            var normPosition = window.PixelToNorm(selectEvent.Position);
            var uiPosition = window.NormToUi(normPosition);

            DoLayout((extents, dialogOrder, element) =>
                element.Select(uiPosition, extents, dialogOrder, (order, target) =>
                    {
                        float z = 1.0f - order / (float)DrawLayer.MaxLayer;
                        var intersectionPoint = new Vector3(normPosition, z);
                        selectEvent.RegisterHit(z, new Selection(intersectionPoint, target));
                    }));
        }

        static (int, int) GetDialogPosition(IDialog dialog, Vector2 size, int uiWidth, int uiHeight)
        {
            int x;
            int y;
            switch (dialog.Positioning)
            {
                case DialogPositioning.Center:
                    x = (uiWidth - (int) size.X) / 2;
                    y = (uiHeight - (int) size.Y) / 2;
                    break;
                case DialogPositioning.Bottom:
                    x = (uiWidth - (int) size.X) / 2;
                    y = uiHeight - (int) size.Y;
                    break;
                case DialogPositioning.Top:
                    x = (uiWidth - (int) size.X) / 2;
                    y = 0;
                    break;
                case DialogPositioning.Left:
                    x = 0;
                    y = (uiHeight - (int) size.Y) / 2;
                    break;
                case DialogPositioning.Right:
                    x = uiWidth - (int) size.X;
                    y = (uiHeight - (int) size.Y) / 2;
                    break;
                case DialogPositioning.BottomLeft:
                    x = 0;
                    y = uiHeight - (int) size.Y;
                    break;
                case DialogPositioning.TopLeft:
                    x = 0;
                    y = 0;
                    break;
                case DialogPositioning.TopRight:
                    x = uiWidth - (int) size.X;
                    y = 0;
                    break;
                case DialogPositioning.BottomRight:
                    x = uiWidth - (int) size.X;
                    y = uiHeight - (int) size.Y;
                    break;
                case DialogPositioning.StatusBar:
                    x = (uiWidth - (int) size.X) / 2;
                    y = UiConstants.StatusBarExtents.Y;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (x, y);
        }
    }
}
