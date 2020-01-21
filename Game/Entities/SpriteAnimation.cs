using System.Collections.Generic;

namespace UAlbion.Game.Entities
{
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

    public static class LargeSpriteAnimations
    {
        public static readonly IDictionary<SpriteAnimation, int[]> Frames = new Dictionary<SpriteAnimation, int[]>
        {
            { SpriteAnimation.WalkN, new[] {  0,  1,  2 } },
            { SpriteAnimation.WalkE, new[] {  3,  4,  5 } },
            { SpriteAnimation.WalkS, new[] {  6,  7,  8 } },
            { SpriteAnimation.WalkW, new[] {  9, 10, 11 } }, //*/

            /* Reverse order
            { SpriteAnimation.WalkN, new[] {  2,  1, 0 } },
            { SpriteAnimation.WalkE, new[] {  5,  4, 3 } },
            { SpriteAnimation.WalkS, new[] {  8,  7, 6 } },
            { SpriteAnimation.WalkW, new[] { 11, 10, 9 } }, //*/

            { SpriteAnimation.SitN,  new[] { 12 } },
            { SpriteAnimation.SitE,  new[] { 13 } },
            { SpriteAnimation.SitS,  new[] { 14 } },
            { SpriteAnimation.SitW,  new[] { 15 } },
            { SpriteAnimation.Sleeping, new[] { 16 } },
        };
    }

    public static class SmallSpriteAnimations
    {
        public static readonly IDictionary<SpriteAnimation, int[]> Frames = new Dictionary<SpriteAnimation, int[]>
        {
            { SpriteAnimation.WalkN, new[] { 0,  1,  2 } },
            { SpriteAnimation.WalkE, new[] { 3,  4,  5 } },
            { SpriteAnimation.WalkS, new[] { 6,  7,  8 } },
            { SpriteAnimation.WalkW, new[] { 9, 10, 11 } },
        };
    }
}