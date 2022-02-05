using System.Collections.Generic;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities;

public static class LargeSpriteAnimations
{
    public static readonly IDictionary<SpriteAnimation, int[]> Frames = new Dictionary<SpriteAnimation, int[]>
    {
        { SpriteAnimation.WalkN, new[] {  0,  1,  2,  1 } },
        { SpriteAnimation.WalkE, new[] {  3,  4,  5,  4 } },
        { SpriteAnimation.WalkS, new[] {  6,  7,  8,  7 } },
        { SpriteAnimation.WalkW, new[] {  9, 10, 11, 10 } }, //*/

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
        { SpriteAnimation.WalkN, new[] { 0,  1,  2,  1 } },
        { SpriteAnimation.WalkE, new[] { 3,  4,  5,  4 } },
        { SpriteAnimation.WalkS, new[] { 6,  7,  8,  7 } },
        { SpriteAnimation.WalkW, new[] { 9, 10, 11, 10 } },
    };
}