using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("dialog_line")]
public class DialogLineEvent : GameEvent
{
    public DialogLineEvent(int line, int? textId)
    {
        Line = line;
        TextId = textId;
    }

    [EventPart("line")] public int Line { get; }
    [EventPart("text_id", true)] public int? TextId { get; }
}