using UAlbion.Formats.Assets;

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
        T Language(string language);
        T Ink(FontColor color);
    }
}
