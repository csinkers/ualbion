using UAlbion.Formats.Assets;

namespace UAlbion.Game.Text
{
    public interface IWordLookup
    {
        WordId Parse(string s);
    }
}