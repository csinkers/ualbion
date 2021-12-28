using System;

namespace UAlbion.Formats.Assets;

public interface IContents : IEquatable<IContents>
{
    SpriteId Icon { get; }
    int IconSubId { get; }
    byte IconAnim { get; }
}