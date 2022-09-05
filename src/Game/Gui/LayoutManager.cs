using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Text;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using static System.FormattableString;

namespace UAlbion.Game.Gui;

public interface ILayoutManager
{
    LayoutNode GetLayout();
    IDictionary<IUiElement, LayoutNode> LastSnapshot { get; }
    void RequestSnapshot();
}

public class LayoutManager : ServiceComponent<ILayoutManager>, ILayoutManager
{
    readonly CollectDialogsEvent _collectDialogsEvent = new();
    readonly SelectionContext _selectionContext = new();

    public IDictionary<IUiElement, LayoutNode> LastSnapshot { get; private set; } =
        new Dictionary<IUiElement, LayoutNode>();

    public LayoutManager()
    {
        On<LayoutEvent>(RenderLayout);
        On<DumpLayoutEvent>(_ => DumpLayout());
        On<ScreenCoordinateSelectEvent>(Select);
    }

    public void RequestSnapshot() => CaptureSnapshot();
    void DoLayout<TContext>(Func<TContext, Rectangle, int, IUiElement, int> action, TContext context)
    {
        int order = (int)DrawLayer.Interface;
        int uiWidth = UiConstants.ActiveAreaExtents.Width;
        int uiHeight = UiConstants.ActiveAreaExtents.Height;
        _collectDialogsEvent.Dialogs.Clear();
        Raise(_collectDialogsEvent);
        _collectDialogsEvent.Dialogs.Sort((x, y) => x.Depth.CompareTo(y.Depth));

        foreach (var dialog in _collectDialogsEvent.Dialogs)
        {
            var size = dialog.GetSize();

            void LayoutDialog(Vector2 dialogSize)
            {
                var (x, y) = GetDialogPosition(dialog, dialogSize, uiWidth, uiHeight);
                order = action(context, new Rectangle(x, y, (int)dialogSize.X, (int)dialogSize.Y), order + 1, dialog);
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

    void RenderLayout(LayoutEvent e)
    {
        var flags = GetVar(CoreVars.User.EngineFlags);
        if ((flags & EngineFlags.SuppressLayout) != 0)
        {
            // If the user specifically requested this layout run then
            // dump out the details of it to the console.
            DumpLayout();
            return;
        }

        DoLayout(static (_, extents, order, element) => element.Render(extents, order, null), 0);
    }

    void Select(ScreenCoordinateSelectEvent e)
    {
        var window = Resolve<IWindowManager>();
        _selectionContext.NormPosition = window.PixelToNorm(e.Position);
        _selectionContext.UiPosition = window.NormToUi(_selectionContext.NormPosition);
        _selectionContext.Selections = e.Selections;
        DoLayout(static (c, extents, order, x) => x.Selection(extents, order, c), _selectionContext);
    }

    static (int, int) GetDialogPosition(IDialog dialog, Vector2 size, int uiWidth, int uiHeight)
    {
        int x;
        int y;
        switch (dialog.Positioning)
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
                y = UiConstants.StatusBarExtents.Y;
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(dialog.Positioning), (int)dialog.Positioning,
                    typeof(DialogPositioning));
        }

        return (x, y);
    }

    public LayoutNode GetLayout()
    {
        var rootNode = new LayoutNode(null, null, UiConstants.UiExtents, 0);
        DoLayout(static (root, extents, order, element) =>
            element.Render(extents, order, new LayoutNode(root, element, extents, order)),
            rootNode);

        return rootNode;
    }

    void DumpLayout()
    {
        var root = GetLayout();
        var sb = new StringBuilder();

        void Aux(LayoutNode node, int level)
        {
            var size = node.Element?.GetSize() ?? Vector2.Zero;
            sb.Append(Invariant($"{node.Order,4} ({node.Extents.X,3}, {node.Extents.Y,3}, {node.Extents.Width,3}, {node.Extents.Height,3}) <{size.X,3}, {size.Y,3}> "));
            sb.Append("".PadLeft(level * 2));
            sb.AppendLine(Invariant($"{node.Element}"));
            foreach (var child in node.Children)
                Aux(child, level + 1);
        }

        Aux(root, 0);
        Info(sb.ToString());
        Raise(new SetClipboardTextEvent(sb.ToString()));
    }

    void CaptureSnapshot()
    {
        var snapshot = new Dictionary<IUiElement, LayoutNode>();
        var root = GetLayout();

        void Aux(LayoutNode node)
        {
            if (node.Element != null)
                snapshot[node.Element] = node;

            foreach (var child in node.Children)
                Aux(child);
        }

        Aux(root);
        LastSnapshot = snapshot;
    }
}