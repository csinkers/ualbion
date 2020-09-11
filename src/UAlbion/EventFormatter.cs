using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Game;

namespace UAlbion
{
    class EventFormatter
    {
        readonly IAssetManager _assets;
        readonly AssetType _textType;
        readonly ushort _context;

        public EventFormatter(IAssetManager assets, AssetType textType, ushort context)
        {
            _assets = assets;
            _textType = textType;
            _context = context;
        }

        public string GetText(IEventNode e)
        {
            var nodeText = e.ToString();
            if (e.Event is BaseTextEvent textEvent)
            {
                var text = _assets.LoadString(
                    new StringId(_textType, _context, textEvent.TextId),
                    GameLanguage.English);

                return $"{nodeText} \"{text}\"";
            }

            return nodeText;
        }
    }
}
