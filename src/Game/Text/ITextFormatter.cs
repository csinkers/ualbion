using System.Collections.Generic;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Text;

public interface ITextFormatter
{
    IText Format(TextId textId, params object[] arguments);
    IText Format(StringId stringId, params object[] arguments);
    IText Format(string templateText, params object[] arguments);
    IText Format(TextId textId, IList<(Token, object)> implicitTokens, params object[] arguments);
    IText Format(StringId stringId, IList<(Token, object)> implicitTokens, params object[] arguments);
    IText Format(string templateText, IList<(Token, object)> implicitTokens, params object[] arguments);

    ITextFormatter NoWrap();
    ITextFormatter Left();
    ITextFormatter Center();
    ITextFormatter Right();
    ITextFormatter Justify();
    ITextFormatter Fat();
    ITextFormatter Block(int blockNumber); // -1 = unlabelled text before the first block
    ITextFormatter Ink(InkId id);
}