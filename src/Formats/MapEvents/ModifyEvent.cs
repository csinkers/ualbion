using System;
using System.ComponentModel;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.MapEvents
{
    public abstract class ModifyEvent : MapEvent
    {
        public static ModifyEvent BaseSerdes(ModifyEvent genericEvent, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var subType = s.EnumU8("SubType", genericEvent?.SubType ?? ModifyType.Unk2);
            return subType switch
            {
                ModifyType.Switch => SwitchEvent.Serdes((SwitchEvent)genericEvent, mapping, s),
                ModifyType.DisableEventChain => DisableEventChainEvent.Serdes((DisableEventChainEvent)genericEvent, s),
                ModifyType.NpcActive => NpcActiveEvent.Serdes((NpcActiveEvent)genericEvent, s),
                ModifyType.AddPartyMember => AddPartyMemberEvent.Serdes((AddPartyMemberEvent)genericEvent, mapping, s),
                ModifyType.InventoryItem => AddRemoveInventoryItemEvent.Serdes((AddRemoveInventoryItemEvent)genericEvent, mapping, s),
                ModifyType.MapLighting => SetMapLightingEvent.Serdes((SetMapLightingEvent)genericEvent, s),
                ModifyType.PartyGold => ChangePartyGoldEvent.Serdes((ChangePartyGoldEvent)genericEvent, s),
                ModifyType.PartyRations => ChangePartyRationsEvent.Serdes((ChangePartyRationsEvent)genericEvent, s),
                ModifyType.Time => ChangeTimeEvent.Serdes((ChangeTimeEvent)genericEvent, s),
                ModifyType.Leader => SetPartyLeaderEvent.Serdes((SetPartyLeaderEvent)genericEvent, mapping, s),
                ModifyType.Ticker => TickerEvent.Serdes((TickerEvent)genericEvent, mapping, s),
                ModifyType.Unk2 => ModifyUnk2Event.Serdes((ModifyUnk2Event)genericEvent, s),
                _ => throw new InvalidEnumArgumentException(nameof(subType), (int)subType, typeof(ModifyType))
            };
        }

        public enum ModifyType : byte
        {
            Switch = 0,
            DisableEventChain = 1,
            Unk2 = 2,
            NpcActive = 4,
            AddPartyMember = 5,
            InventoryItem = 6,
            MapLighting = 0xB,
            PartyGold = 0xF,
            PartyRations = 0x10,
            Time = 0x12,
            Leader = 0x1A,
            Ticker = 0x1C
        }

        public abstract ModifyType SubType { get; }
        public override MapEventType EventType => MapEventType.Modify;
    }
}
