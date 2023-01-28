using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input;

public class TextEntryInputMode : Component
{
    public TextEntryInputMode() => On<KeyboardInputEvent>(OnInput);
    void OnInput(KeyboardInputEvent e)
    {
        foreach (var c in e.KeyCharPresses)
            Raise(new TextEntryCharEvent(c));

        foreach (var ke in e.KeyEvents)
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