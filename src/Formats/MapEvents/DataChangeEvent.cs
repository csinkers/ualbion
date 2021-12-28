using System;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.MapEvents;

public abstract class DataChangeEvent : MapEvent
{
    public override MapEventType EventType => MapEventType.DataChange;
    public abstract ChangeProperty ChangeProperty { get; }

    public static DataChangeEvent Serdes(DataChangeEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (s.IsWriting() && e == null) throw new ArgumentNullException(nameof(e));

        ChangeProperty property = s.EnumU8(nameof(ChangeProperty), e?.ChangeProperty ?? ChangeProperty.Unk0);
        return property switch
        {
            ChangeProperty.Unk0 => ChangeUnk0Event.Serdes((ChangeUnk0Event)e, s),
            ChangeProperty.Health => ChangeHealthEvent.Serdes((ChangeHealthEvent)e, mapping, s),
            ChangeProperty.Mana => ChangeManaEvent.Serdes((ChangeManaEvent)e, mapping, s),
            ChangeProperty.Status => ChangeStatusEvent.Serdes((ChangeStatusEvent)e, mapping, s),
            ChangeProperty.Language => ChangeLanguageEvent.Serdes((ChangeLanguageEvent)e, mapping, s),
            ChangeProperty.Experience => ChangeExperienceEvent.Serdes((ChangeExperienceEvent)e, mapping, s),
            ChangeProperty.UnkB => ChangeUnkBEvent.Serdes((ChangeUnkBEvent)e, s),
            ChangeProperty.UnkC => ChangeUnkCEvent.Serdes((ChangeUnkCEvent)e, s),
            ChangeProperty.Item => ChangeItemEvent.Serdes((ChangeItemEvent)e, mapping, s),
            ChangeProperty.Gold => ChangeGoldEvent.Serdes((ChangeGoldEvent)e, mapping, s),
            ChangeProperty.Food => ChangeFoodEvent.Serdes((ChangeFoodEvent)e, mapping, s),
            _ => throw new FormatException($"Unexpected data change type \"{property}\"")
        };
    }
}