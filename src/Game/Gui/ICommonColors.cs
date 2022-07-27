using System.Collections.Generic;
using UAlbion.Api.Visual;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Gui;

public interface ICommonColors
{
    IDictionary<CommonColor, uint> Palette { get; }
    IReadOnlyTexture<byte> BorderTexture { get; }
    Region GetRegion(CommonColor color);
}