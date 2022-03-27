namespace UAlbion.Formats.MapEvents;

public enum DataChangeTarget : byte
{
    Leader = 0,
    Everyone = 1,
    SpecificMember = 2, // byte5 has slot num
    Attacker = 3,
    Target = 4,
    Npc = 5,
    Inventory = 6, // related to inventory background images?
    LastMessageTarget = 7
}