using System.Collections.Generic;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Text
{
    public class UiTextBuilder : UiElement, IText, ITextBuilder<UiTextBuilder>
    {
        readonly IList<(Token, object)> _implicitTokens = new List<(Token, object)>();
        readonly StringId _stringId;
        GameLanguage? _language;

        public UiTextBuilder(StringId stringId) => _stringId = stringId;
        public int Version => _implicitTokens.Count + 1;
        public IEnumerable<TextBlock> Get()
        {
            var tf = Resolve<ITextFormatter>();
            return tf.Format(_stringId, _implicitTokens, _language).Get();
        }

        public UiTextBuilder NoWrap() { _implicitTokens.Add((Token.NoWrap, null)); return this; }
        public UiTextBuilder Left() { _implicitTokens.Add((Token.Left, null)); return this; }
        public UiTextBuilder Center() { _implicitTokens.Add((Token.Centre, null)); return this; }
        public UiTextBuilder Right() { _implicitTokens.Add((Token.Right, null)); return this; }
        public UiTextBuilder Justify() { _implicitTokens.Add((Token.Justify, null)); return this; }
        public UiTextBuilder Fat() { _implicitTokens.Add((Token.Fat, null)); return this; }
        public UiTextBuilder Ink(FontColor color) { _implicitTokens.Add((Token.Ink, (int)color)); return this; }
        public UiTextBuilder Language(GameLanguage language) { _language = language; return this; }

        protected override void Subscribed()
        {
            if (Children.Count != 0)
                return;

            AttachChild(new UiText(this));
        }
    }
}