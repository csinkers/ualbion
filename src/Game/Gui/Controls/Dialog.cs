using UAlbion.Game.Events;

namespace UAlbion.Game.Gui.Controls;

public interface IDialog : IUiElement
{
    int Depth { get; }
    DialogPositioning Positioning { get; }
}

public class Dialog : UiElement, IDialog
{
    protected Dialog(DialogPositioning position, int depth = 0)
    {
        On<CollectDialogsEvent>(e => e.AddDialog(this));
        Positioning = position;
        Depth = depth;
    }

    public int Depth { get; }
    public DialogPositioning Positioning { get; }
}