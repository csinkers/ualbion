using System.Collections.Generic;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Text;

public class CustomisedTextFormatter : ITextFormatter
{
    readonly TextFormatter _formatter;
    readonly List<(Token, object)> _implicitTokens = [];
    BlockId? _blockFilter;

    public CustomisedTextFormatter(TextFormatter formatter) => _formatter = formatter;

    public IText Format(TextId textId, params object[] arguments)
        => Filter(_formatter.Format(textId, _implicitTokens, arguments));

    public IText Format(StringId stringId, params object[] arguments) 
        => Filter(_formatter.Format(stringId, _implicitTokens, arguments));

    public IText Format(string templateText, params object[] arguments)
        => Filter(_formatter.Format(templateText, _implicitTokens, arguments));

    public IText Format(TextId textId, IList<(Token, object)> implicitTokens, params object[] arguments)
        => Filter(_formatter.Format(textId, implicitTokens, arguments));

    public IText Format(StringId stringId, IList<(Token, object)> implicitTokens, params object[] arguments)
        => Filter(_formatter.Format(stringId, implicitTokens, arguments));

    public IText Format(string templateText, IList<(Token, object)> implicitTokens, params object[] arguments)
        => Filter(_formatter.Format(templateText, implicitTokens, arguments));

    IText Filter(IText text) => _blockFilter != null
        ? new TextFilter(x => x.BlockId == _blockFilter.Value) { Source = text }
        : text;

    public ITextFormatter NoWrap() { _implicitTokens.Add((Token.NoWrap, null)); return this; }
    public ITextFormatter Left() { _implicitTokens.Add((Token.Left, null)); return this; }
    public ITextFormatter Center() { _implicitTokens.Add((Token.Centre, null)); return this; }
    public ITextFormatter Right(){ _implicitTokens.Add((Token.Right, null)); return this; }
    public ITextFormatter Justify(){ _implicitTokens.Add((Token.Justify, null)); return this; }
    public ITextFormatter Fat() { _implicitTokens.Add((Token.Fat, null)); return this; }
    public ITextFormatter Ink(InkId id) { _implicitTokens.Add((Token.Ink, id)); return this; }
    public ITextFormatter Block(BlockId blockId) { _blockFilter = blockId; return this; }
}