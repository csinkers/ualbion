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
                ModifyType.SetTemporarySwitch => SetTemporarySwitchEvent.Serdes((SetTemporarySwitchEvent)genericEvent, mapping, s),
                ModifyType.DisableEventChain => DisableEventChainEvent.Serdes((DisableEventChainEvent)genericEvent, s),
                ModifyType.SetNpcActive => SetNpcActiveEvent.Serdes((SetNpcActiveEvent)genericEvent, s),
                ModifyType.AddPartyMember => AddPartyMemberEvent.Serdes((AddPartyMemberEvent)genericEvent, mapping, s),
                ModifyType.AddRemoveInventoryItem => AddRemoveInventoryItemEvent.Serdes((AddRemoveInventoryItemEvent)genericEvent, mapping, s),
                ModifyType.SetMapLighting => SetMapLightingEvent.Serdes((SetMapLightingEvent)genericEvent, s),
                ModifyType.ChangePartyGold => ChangePartyGoldEvent.Serdes((ChangePartyGoldEvent)genericEvent, s),
                ModifyType.ChangePartyRations => ChangePartyRationsEvent.Serdes((ChangePartyRationsEvent)genericEvent, s),
                ModifyType.ChangeTime => ChangeTimeEvent.Serdes((ChangeTimeEvent)genericEvent, s),
                ModifyType.SetPartyLeader => SetPartyLeaderEvent.Serdes((SetPartyLeaderEvent)genericEvent, mapping, s),
                ModifyType.SetTicker => SetTickerEvent.Serdes((SetTickerEvent)genericEvent, mapping, s),
                ModifyType.Unk2 => DummyModifyEvent.Serdes((DummyModifyEvent)genericEvent, s),
                _ => throw new InvalidEnumArgumentException(nameof(subType), (int)subType, typeof(ModifyType))
            };
        }

        public enum ModifyType : byte
        {
            SetTemporarySwitch = 0,
            DisableEventChain = 1,
            Unk2 = 2,
            SetNpcActive = 4,
            AddPartyMember = 5,
            AddRemoveInventoryItem = 6,
            SetMapLighting = 0xB,
            ChangePartyGold = 0xF,
            ChangePartyRations = 0x10,
            ChangeTime = 0x12,
            SetPartyLeader = 0x1A,
            SetTicker = 0x1C
        }

        public abstract ModifyType SubType { get; }
        public override MapEventType EventType => MapEventType.Modify;
    }
}
