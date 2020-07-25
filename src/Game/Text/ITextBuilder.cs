using UAlbion.Formats;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Text
{
    public interface ITextBuilder<out T>
    {
        T NoWrap();
        T Left();
        T Center();
        T Right();
        T Justify();
        T Fat();
        T Language(GameLanguage language);
        T Ink(FontColor color);
    }
}