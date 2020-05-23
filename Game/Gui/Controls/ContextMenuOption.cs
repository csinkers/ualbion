using UAlbion.Api;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Controls
{
    public enum ContextMenuGroup
    {
        Actions, // Examine, talk, take, manipulate etc
        Actions2, // Advance party, any others?
        System, // Main menu, rest etc
    }

    public class ContextMenuOption
    {
        public ContextMenuOption(IText text, IEvent @event, ContextMenuGroup group)
        {
            Text = text;
            Event = @event;
            Group = group;
        }

        public IText Text { get; }
        public IEvent Event { get; }
        public ContextMenuGroup Group { get; }
    }
}
