using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("set_fx_volume")]
public class SetFxVolumeEvent : GameEvent
{
    public SetFxVolumeEvent(int value)
    {
        Value = value;
    }

    [EventPart("value", "The volume level, from 0 to 127.")]
    public int Value { get; }
}