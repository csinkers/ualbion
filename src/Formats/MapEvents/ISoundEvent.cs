using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

public interface ISoundEvent
{
    SampleId SoundId { get; } // [0..78], 153
    byte Volume { get; } // [0..150]
    byte RestartProbability { get; } // [0..100]
    byte Unk3 { get; } // [0..100] (multiples of 5)
    ushort FrequencyOverride { get; } // 0,8, [5..22]*1000
    SoundMode Mode { get; }
}