using System.Collections.Generic;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities;

public static class LargeSpriteAnimations
{
    public static readonly IDictionary<SpriteAnimation, int[]> Frames = new Dictionary<SpriteAnimation, int[]>
    {
        { SpriteAnimation.WalkN, [0,  1,  2,  1] },
        { SpriteAnimation.WalkE, [3,  4,  5,  4] },
        { SpriteAnimation.WalkS, [6,  7,  8,  7] },
        { SpriteAnimation.WalkW, [9, 10, 11, 10] }, //*/

        { SpriteAnimation.SitN, [12] },
        { SpriteAnimation.SitE, [13] },
        { SpriteAnimation.SitS, [14] },
        { SpriteAnimation.SitW, [15] },
        { SpriteAnimation.Sleeping, [16] },
    };
}

public static class SmallSpriteAnimations
{
    public static readonly IDictionary<SpriteAnimation, int[]> Frames = new Dictionary<SpriteAnimation, int[]>
    {
        { SpriteAnimation.WalkN, [0,  1,  2,  1] },
        { SpriteAnimation.WalkE, [3,  4,  5,  4] },
        { SpriteAnimation.WalkS, [6,  7,  8,  7] },
        { SpriteAnimation.WalkW, [9, 10, 11, 10] },
    };
}