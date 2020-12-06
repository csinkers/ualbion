using System;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats
{
    public class EventFormatter
    {
        readonly Func<StringId, string> _stringLoadFunc;
        readonly AssetId _textSourceId;

        public EventFormatter(Func<StringId, string> stringLoadFunc, AssetId textSourceId)
        {
            _stringLoadFunc = stringLoadFunc;
            _textSourceId = textSourceId;
        }

        public string GetText(IEventNode e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            var nodeText = e.ToString();
            if (e.Event is TextEvent textEvent)
            {
                var text = _stringLoadFunc(new StringId(_textSourceId, textEvent.TextId));
                return $"{nodeText} // \"{text}\"";
            }

            return nodeText;
        }
    }
}
