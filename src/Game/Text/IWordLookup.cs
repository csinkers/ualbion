using System.Collections.Generic;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Text;

public interface IWordLookup
{
    WordId Parse(string s);
    IEnumerable<WordId> GetHomonyms(WordId word);
    string GetText(WordId id, string language);
}