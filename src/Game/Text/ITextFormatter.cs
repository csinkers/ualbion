using System.Collections.Generic;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Text
{
    public interface ITextFormatter
    {
        IText Format(StringId stringId, params object[] arguments);
        IText Format(string templateText, params object[] arguments);
        IText Format(StringId stringId, IList<(Token, object)> implicitTokens, GameLanguage? language, params object[] arguments);
        IText Format(string templateText, IList<(Token, object)> implicitTokens, GameLanguage? language, params object[] arguments);

        ITextFormatter NoWrap();
        ITextFormatter Left();
        ITextFormatter Center();
        ITextFormatter Right();
        ITextFormatter Justify();
        ITextFormatter Fat();
        ITextFormatter Language(GameLanguage language);
        ITextFormatter Ink(FontColor color);
    }
}