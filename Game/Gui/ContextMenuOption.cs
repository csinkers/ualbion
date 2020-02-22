using UAlbion.Api;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui
{
    public enum ContextMenuGroup
    {
        Actions, // Examine, talk, take, manipulate etc
        Actions2, // Advance party, any others?
        System, // Main menu, rest etc
    }

    public class ContextMenuOption
    {
        public ContextMenuOption(ITextSource text, IEvent @event, ContextMenuGroup @group)
        {
            Text = text;
            Event = @event;
            Group = @group;
        }

        public ITextSource Text { get; }
        public IEvent Event { get; }
        public ContextMenuGroup Group { get; }
    }
}
