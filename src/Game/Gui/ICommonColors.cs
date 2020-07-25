using System.Collections.Generic;
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui
{
    public interface ICommonColors
    {
        IDictionary<CommonColor, uint> Palette { get; }
        ITexture BorderTexture { get; }
    }
}
