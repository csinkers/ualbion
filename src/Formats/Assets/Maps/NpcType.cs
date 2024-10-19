namespace UAlbion.Formats.Assets.Maps;

public enum NpcType : byte
{
    Party   = 0, // PartyGfx, fires Id as party member event set (i.e. set id Id+980)
    Npc     = 1, // NpcGfx,   fires Id as EventSet
    Monster = 2, // NpcGfx,   nothing
    Prop = 3, // NpcGfx,   nothing - fireballs etc
}