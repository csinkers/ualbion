using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events;

[Event("wavelib")]
public class WaveLibEvent : GameEvent, IVerboseEvent
{
    public WaveLibEvent(SongId songId, int instrument, int velocity, int note)
    {
        SongId = songId;
        Instrument = instrument;
        Velocity = velocity;
        Note = note;
    }

    [EventPart("song")] public SongId SongId { get; }
    [EventPart("instrument")] public int Instrument { get; }
    [EventPart("velocity")] public int Velocity { get; }
    [EventPart("note")] public int Note { get; }
}