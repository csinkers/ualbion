using UAlbion.Formats.Ids;

namespace UAlbion.Game.Text;

public interface IWordLookup
{
    WordId Parse(string s);
}