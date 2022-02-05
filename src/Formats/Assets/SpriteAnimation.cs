namespace UAlbion.Formats.Assets;

public enum SpriteAnimation
{
    WalkN = 0,
    WalkE = 1,
    WalkS = 2,
    WalkW = 3,
    MaxWalk = 3,

    SitN  = 4, // Sitting and sleeping animations only exist for large sprites
    SitE  = 5,
    SitS  = 6,
    SitW  = 7,
    Sleeping = 8,
    Max = 8
}