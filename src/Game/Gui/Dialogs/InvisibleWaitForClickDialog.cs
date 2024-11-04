using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Dialogs;

public class InvisibleWaitForClickDialog : ModalDialog
{
    readonly AlbionTaskCore _source = new(nameof(InvisibleWaitForClickDialog));
    public AlbionTask Task => _source.UntypedTask;

    public InvisibleWaitForClickDialog( int depth = 0) : base(DialogPositioning.Top, depth)
    {
        On<DismissMessageEvent>(_ => Close());
        On<CloseWindowEvent>(_ => Close());
        On<UiLeftClickEvent>(e => { Close(); e.Propagating = false; });
        On<UiRightClickEvent>(e => { Close(); e.Propagating = false; });

        AttachChild(new FixedPosition(UiConstants.UiExtents, null));
    }

    void Close()
    {
        Remove();
        _source.Complete();
    }
}