namespace UAlbion.Formats.Assets.Maps;

public enum RestMode
{
    Wait = 0, // City - Waiting
    RestEightHours = 1, // Dungeon - Rest for eight hours
    RestUntilDawn = 2, // Wilderness - Rest until dawn
    NoResting = 3 // Interior - No resting
}