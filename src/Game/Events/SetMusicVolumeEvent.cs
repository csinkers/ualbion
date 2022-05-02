using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("set_music_volume")]
public class SetMusicVolumeEvent : GameEvent
{
    public SetMusicVolumeEvent(int value)
    {
        Value = value;
    }

    [EventPart("value", "The volume level, from 0 to 127.")]
    public int Value { get; }
}