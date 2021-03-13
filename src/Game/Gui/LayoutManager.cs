using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui
{
    public interface ILayoutManager
    {
        LayoutNode GetLayout();
    }

    public class LayoutManager : ServiceComponent<ILayoutManager>, ILayoutManager
    {
        public LayoutManager()
        {
            On<LayoutEvent>(RenderLayout);
            On<DumpLayoutEvent>(DumpLayout);
            OnAsync<ScreenCoordinateSelectEvent, Selection>(Select);
        }

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
                    order = action(new Rectangle(x, y, (int)dialogSize.X, (int)dialogSize.Y), order + 1, dialog);
                }
                LayoutDialog(size);

#if DEBUG
                var sizeAfter = dialog.GetSize(); // Hacky fix for first frame being different
                if (sizeAfter != size)
                {
                    ApiUtil.Assert($"Dialog \"{dialog}\" changed size after rendering, from {size} to {sizeAfter}.");
                    LayoutDialog(sizeAfter);
                }
#endif
            }
        }

        void RenderLayout(LayoutEvent e) => DoLayout((extents, order, element) => element.Render(extents, order));

        bool Select(ScreenCoordinateSelectEvent selectEvent, Action<Selection> continuation)
        {
            var window = Resolve<IWindowManager>();
            var normPosition = window.PixelToNorm(selectEvent.Position);
            var uiPosition = window.NormToUi(normPosition);

            DoLayout((extents, dialogOrder, element) =>
                element.Select(uiPosition, extents, dialogOrder, (order, target) =>
                    {
                        float z = 1.0f - order / (float)DrawLayer.MaxLayer;
                        var intersectionPoint = new Vector3(normPosition, z);
                        continuation(new Selection(intersectionPoint, z, target));
                    }));
            return true;
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
                    throw new InvalidEnumArgumentException(nameof(dialog.Positioning), (int)dialog.Positioning, typeof(DialogPositioning));
            }

            return (x, y);
        }

        public LayoutNode GetLayout()
        {
            var rootNode = new LayoutNode(null, null, UiConstants.UiExtents, 0);
            DoLayout((extents, order, element) => element.Layout(extents, order, new LayoutNode(rootNode, element, extents, order)));
            return rootNode;
        }

        void DumpLayout(DumpLayoutEvent _)
        {
            var root = GetLayout();
            var sb = new StringBuilder();

            void Aux(LayoutNode node, int level)
            {
                var size = node.Element?.GetSize() ?? Vector2.Zero;
                sb.Append($"{node.Order,4} ({node.Extents.X,3}, {node.Extents.Y,3}, {node.Extents.Width,3}, {node.Extents.Height,3}) <{size.X,3}, {size.Y,3}> ");
                sb.Append("".PadLeft(level * 2));
                sb.AppendLine($"{node.Element}");
                foreach (var child in node.Children)
                    Aux(child, level + 1);
            }

            Aux(root, 0);
            Raise(new LogEvent(LogEvent.Level.Info, sb.ToString()));
            Raise(new SetClipboardTextEvent(sb.ToString()));
        }
    }
}
