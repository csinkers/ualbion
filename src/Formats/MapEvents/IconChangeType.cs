namespace UAlbion.Formats.MapEvents;

public enum IconChangeType : byte
{
    Unknown = 0xff,
    Underlay = 0,
    Overlay = 1,
    Wall = 2,
    Floor = 3,
    Ceiling = 4,
    NpcMovement = 5, // X = NpcId, Values: 0=Waypoints, 1=Random, 2=Stay, 3=Follow
    NpcSprite = 6, // X = NpcId
    Chain = 7,
    BlockHard = 8, // Objects are in BLKLIST#.XLD (overwrite existing tiles)
    BlockSoft = 9, // Objects are in BLKLIST#.XLD (don't overwrite)
    Trigger = 0xA, // ???? Might not be 0xA
}