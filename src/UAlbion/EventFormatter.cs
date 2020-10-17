using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game;

namespace UAlbion
{
    class EventFormatter
    {
        readonly IAssetManager _assets;
        readonly AssetId _textSourceId;

        public EventFormatter(IAssetManager assets, AssetId textSourceId)
        {
            _assets = assets;
            _textSourceId = textSourceId;
        }

        public string GetText(IEventNode e)
        {
            var nodeText = e.ToString();
            if (e.Event is TextEvent textEvent)
            {
                var text = _assets.LoadString(new StringId(_textSourceId, textEvent.TextId));

                return $"{nodeText} \"{text}\"";
            }

            return nodeText;
        }
    }
}
