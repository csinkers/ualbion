using Veldrid.Sdl2;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Input;

public class TextEntryInputMode : Component
{
    public TextEntryInputMode() => On<KeyboardInputEvent>(OnInput);
    void OnInput(KeyboardInputEvent e)
    {
        foreach (var c in e.InputEvents)
            Raise(new TextEntryCharEvent(c));

        foreach (var ke in e.KeyEvents)
        {
            if (!ke.Down)
                continue;

            switch (ke.Physical)
            {
                case Key.Backspace: Raise(new TextEntryBackspaceEvent()); break;
                case Key.Escape: Raise(new TextEntryAbortEvent()); break;
                case Key.Return: Raise(new TextEntryCompleteEvent()); break;
            }
        }
    }
}
