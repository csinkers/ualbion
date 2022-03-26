namespace UAlbion.Formats.MapEvents;

public enum DataChangeTarget : byte
{
    PartyLeader = 0,
    AllMembers = 1,
    SpecificMember = 2, // byte5 has slot num
    Unk3 = 3, // based on unknown global state
    Unk4 = 4, // based on unknown global state
    Npc = 5,
    InventoryPic = 6, // related to inventory background images?
    Unk7 = 7 // based on unknown global state
}