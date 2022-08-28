using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input;

public class TextEntryInputMode : Component
{
    public TextEntryInputMode() => On<InputEvent>(OnInput);
    void OnInput(InputEvent e)
    {
        foreach (var c in e.Snapshot.KeyCharPresses)
            Raise(new TextEntryCharEvent(c));

        foreach (var ke in e.Snapshot.KeyEvents)
        {
            if (!ke.Down)
                continue;

            switch (ke.Key)
            {
                case Key.BackSpace: Raise(new TextEntryBackspaceEvent()); break;
                case Key.Escape: Raise(new TextEntryAbortEvent()); break;
                case Key.Enter: Raise(new TextEntryCompleteEvent()); break;
            }
        }
    }
}