using System.Collections.Generic;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Text;

public class UiTextBuilder : UiElement, IText, ITextBuilder<UiTextBuilder>
{
    readonly List<(Token, object)> _implicitTokens = [];
    readonly StringId _stringId;
    string _language;

    public UiTextBuilder(TextId textId) => _stringId = new StringId(textId);
    public UiTextBuilder(StringId stringId) => _stringId = stringId;
    public int Version => _implicitTokens.Count + 1;
    public IEnumerable<TextBlock> GetBlocks()
    {
        var tf = Resolve<ITextFormatter>();
        return tf.Format(_stringId, _implicitTokens, _language).GetBlocks();
    }

    public UiTextBuilder NoWrap() { _implicitTokens.Add((Token.NoWrap, null)); return this; }
    public UiTextBuilder Left() { _implicitTokens.Add((Token.Left, null)); return this; }
    public UiTextBuilder Center() { _implicitTokens.Add((Token.Centre, null)); return this; }
    public UiTextBuilder Right() { _implicitTokens.Add((Token.Right, null)); return this; }
    public UiTextBuilder Justify() { _implicitTokens.Add((Token.Justify, null)); return this; }
    public UiTextBuilder Fat() { _implicitTokens.Add((Token.Fat, null)); return this; }
    public UiTextBuilder Ink(InkId id) { _implicitTokens.Add((Token.Ink, id)); return this; }
    public UiTextBuilder Language(string language) { _language = language; return this; }

    protected override void Subscribed()
    {
        if (Children.Count != 0)
            return;

        AttachChild(new UiText(this));
    }
}