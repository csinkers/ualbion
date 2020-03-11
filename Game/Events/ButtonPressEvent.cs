using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("button_press", "Emitted when a button is pressed")]
    public class ButtonPressEvent : GameEvent
    {
        [EventPart("id", "The identifier of the button that was pressed")]
        public string ButtonId { get; }

        public ButtonPressEvent(string buttonId)
        {
            ButtonId = buttonId;
        }
    }
}
