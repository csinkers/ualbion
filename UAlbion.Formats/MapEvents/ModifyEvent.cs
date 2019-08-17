using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class ModifyEvent : MapEvent
    {
        public ModifyEvent(BinaryReader br, int id)
        {
            throw new System.NotImplementedException();
        }

        public enum ModifyType
        {
            SetTemporarySwitch = 0,
            DisableEventChain = 1,
            ActivateNpc = 4,
            AddPartyMember = 5,
            AddRemoveInventoryItem = 6,
            SetMapLighting = 0xB,
            ChangePartyGold = 0xF,
            ChangePartyRations = 0x10,
            ChangeTime = 0x12,
            SetPartyLeader = 0x1A,
            SetTicker = 0x1C
        }

        public override EventType Type => EventType.Modify;

    }
}