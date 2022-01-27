using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("sound", "Play a sound sample")] // USED IN SCRIPT
public class SoundEvent : MapEvent, ISoundEvent
{
    SoundEvent() { }
    public SoundEvent(
        SampleId soundId,
        byte volume,
        byte restartProbability,
        byte unk3,
        ushort frequencyOverride,
        SoundMode mode)
    {
        SoundId = soundId;
        Volume = volume;
        RestartProbability = restartProbability;
        Unk3 = unk3;
        FrequencyOverride = frequencyOverride;
        Mode = mode;
    }

    public static SoundEvent Serdes(SoundEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new SoundEvent();
        e.Mode = s.EnumU8(nameof(Mode), e.Mode);
        e.SoundId = SampleId.SerdesU8(nameof(SoundId), e.SoundId, mapping, s);
        e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
        e.Volume = s.UInt8(nameof(Volume), e.Volume);
        e.RestartProbability = s.UInt8(nameof(RestartProbability), e.RestartProbability);
        e.FrequencyOverride = s.UInt16(nameof(FrequencyOverride), e.FrequencyOverride);
        int zeroed = s.UInt16(null, 0);
        s.Assert(e.Unk3 <= 100, "SoundEvent: Expected unk3 to be <= 100");
        s.Assert(e.Volume <= 150, "SoundEvent: Expected volume to be <= 150");
        s.Assert(e.RestartProbability <= 102, "SoundEvent: Expected restart probability to be <= 102");
        s.Assert(zeroed == 0, "SoundEvent: Expected field 8 to be 0");
        return e;
    }

    [EventPart("sound")] public SampleId SoundId { get; private set; } // [0..78], 153
    [EventPart("vol")] public byte Volume { get; private set; } // [0..150]
    [EventPart("restart_prob")] public byte RestartProbability { get; private set; } // [0..100]
    [EventPart("unk3")] public byte Unk3 { get; private set; } // [0..100] (multiples of 5)
    [EventPart("freq")] public ushort FrequencyOverride { get; private set; } // 0,8, [5..22]*1000
    [EventPart("mode", true, SoundMode.GlobalOneShot)] public SoundMode Mode { get; private set; }

    // Disabled default values for now to ensure the script text round-trips close enough
    // [EventPart("vol", true, (byte)100)] public byte Volume { get; private set; } // [0..150]
    // [EventPart("restart_prob", true, (byte)0)] public byte RestartProbability { get; private set; } // [0..100]
    // [EventPart("unk3", true, (byte)0)] public byte Unk3 { get; private set; } // [0..100] (multiples of 5)
    // [EventPart("freq", true, (ushort)0)] public ushort FrequencyOverride { get; private set; } // 0,8, [5..22]*1000
    // [EventPart("mode", true, SoundMode.GlobalOneShot)] public SoundMode Mode { get; private set; }

    public override MapEventType EventType => MapEventType.Sound;
}

[Event("sound_effect", "Play a sound sample")] // USED IN SCRIPT
public class SoundEffectEvent : Event, ISoundEvent
{
    SoundEffectEvent() { }
    public SoundEffectEvent(
        SampleId soundId,
        byte volume,
        byte restartProbability,
        byte unk3,
        ushort frequencyOverride,
        SoundMode mode)
    {
        SoundId = soundId;
        Volume = volume;
        RestartProbability = restartProbability;
        Unk3 = unk3;
        FrequencyOverride = frequencyOverride;
        Mode = mode;
    }

    [EventPart("sound")] public SampleId SoundId { get; } // [0..78], 153
    [EventPart("vol")] public byte Volume { get; } // [0..150]
    [EventPart("restart_prob")] public byte RestartProbability { get; } // [0..100]
    [EventPart("unk3")] public byte Unk3 { get; } // [0..100] (multiples of 5)
    [EventPart("freq")] public ushort FrequencyOverride { get; } // 0,8, [5..22]*1000
    [EventPart("mode", true, SoundMode.GlobalOneShot)] public SoundMode Mode { get; }
}