﻿using System.Collections.Generic;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Text
{
    public class CustomisedTextFormatter : ITextFormatter
    {
        readonly TextFormatter _formatter;
        readonly IList<(Token, object)> _implicitTokens = new List<(Token, object)>();

        public CustomisedTextFormatter(TextFormatter formatter) => _formatter = formatter;

        public IText Format(TextId textId, params object[] arguments)
            => _formatter.Format(textId, _implicitTokens, arguments);

        public IText Format(StringId stringId, params object[] arguments) 
            => _formatter.Format(stringId, _implicitTokens, arguments);

        public IText Format(string template, params object[] arguments)
            => _formatter.Format(template, _implicitTokens, arguments);

        public IText Format(TextId textId, IList<(Token, object)> implicitTokens, params object[] arguments)
            => _formatter.Format(textId, implicitTokens, arguments);

        public IText Format(StringId stringId, IList<(Token, object)> implicitTokens, params object[] arguments)
            => _formatter.Format(stringId, implicitTokens, arguments);

        public IText Format(string template, IList<(Token, object)> implicitTokens, params object[] arguments)
            => _formatter.Format(template, implicitTokens, arguments);

        public ITextFormatter NoWrap() { _implicitTokens.Add((Token.NoWrap, null)); return this; }
        public ITextFormatter Left(){ _implicitTokens.Add((Token.Left, null)); return this; }
        public ITextFormatter Center() { _implicitTokens.Add((Token.Centre, null)); return this; }
        public ITextFormatter Right(){ _implicitTokens.Add((Token.Right, null)); return this; }
        public ITextFormatter Justify(){ _implicitTokens.Add((Token.Justify, null)); return this; }
        public ITextFormatter Fat() { _implicitTokens.Add((Token.Fat, null)); return this; }
        public ITextFormatter Ink(FontColor color) { _implicitTokens.Add((Token.Ink, (int)color)); return this; }
    }
}
