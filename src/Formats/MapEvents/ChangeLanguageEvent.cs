using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("change_language")]
public class ChangeLanguageEvent : MapEvent, IDataChangeEvent
{
    public override MapEventType EventType => MapEventType.DataChange;
    public ChangeProperty ChangeProperty => ChangeProperty.Language;
    [EventPart("target")] public TargetId Target { get; private set; }
    [EventPart("language")] public PlayerLanguage Language { get; private set; }
    [EventPart("op")] public NumericOperation Operation { get; private set; }
    [EventPart("amount", true, (ushort)0)] public ushort Amount { get; private set; }
    [EventPart("random", true, false)] public bool IsRandom { get; private set; }

    ChangeLanguageEvent() { }
    public ChangeLanguageEvent(TargetId target, PlayerLanguage language, NumericOperation operation, ushort amount = 0, bool isRandom = false)
    {
        Target = target;
        Language = language;
        Operation = operation;
        Amount = amount;
        IsRandom = isRandom;
    }

    public static ChangeLanguageEvent Serdes(ChangeLanguageEvent e, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new ChangeLanguageEvent();
        var (targetType, targetId) = DataChangeEvent.UnpackTargetId(e.Target);
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);                   // 2
        targetType  = s.EnumU8(nameof(Target), targetType);                       // 3
        e.IsRandom  = s.UInt8(nameof(IsRandom), (byte)(e.IsRandom ? 1 : 0)) != 0; // 4
        targetId    = s.UInt8("TargetId", targetId);                              // 5
        e.Language  = s.EnumU8(nameof(Language), e.Language);                     // 6
        e.Target = DataChangeEvent.PackTargetId(targetType, targetId);

        int zeroed = s.UInt8(null, 0);
        zeroed += s.UInt16(null, 0);
        s.Assert(zeroed == 0, "ChangeLanguageEvent: Expected bytes 7 through 10 to be zero in ChangeLanguageEvent");
        return e;
    }
}