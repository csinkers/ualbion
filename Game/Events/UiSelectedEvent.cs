using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Game.Gui;

namespace UAlbion.Game.Events
{
    public class UiSelectedEvent : GameEvent, IVerboseEvent
    {
        public IReadOnlyList<IUiElement> SelectedItems { get; }
        public IReadOnlyList<IUiElement> FocusedItems { get; }
        public IReadOnlyList<IUiElement> BlurredItems { get; }

        public UiSelectedEvent(IEnumerable<IUiElement> selectedItems, IEnumerable<IUiElement> focused, IEnumerable<IUiElement> blurred)
        {
            SelectedItems = new List<IUiElement>(selectedItems);
            FocusedItems = new List<IUiElement>(focused);
            BlurredItems = new List<IUiElement>(blurred);
        }
    }
}