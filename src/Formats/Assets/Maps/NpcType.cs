namespace UAlbion.Formats.Assets.Maps;

public enum NpcType
{
    Party = 0,    // PartyGfx, fires Id as party member event set (i.e. set id Id+980)
    Npc = 1,      // NpcGfx,   fires Id as EventSet
    Monster = 2,  // NpcGfx,   nothing
    Prop = 3, // NpcGfx,   nothing - fireballs etc
    Unk4 = 4, // NpcGfx,   can't talk (option appears, but does nothing. SimpleMsg still works)
    Unk5 = 5, // Same as 4?
    Unk6 = 6, // Same as 4+5
    Unk7 = 7,
}