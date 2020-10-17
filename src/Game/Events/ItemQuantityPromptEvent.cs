using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    [Event("prompt:item_quantity")]
    public class ItemQuantityPromptEvent : IAsyncEvent<int>
    {
        public ItemQuantityPromptEvent(StringId text, SpriteId icon, int max, bool useTenths)
        {
            Text = text;
            Icon = icon;
            Max = max;
            UseTenths = useTenths;
        }

        [EventPart("text")] public StringId Text { get; }
        [EventPart("icon")] public SpriteId Icon { get; }
        [EventPart("max")] public int Max { get; }
        [EventPart("useTenths")] public bool UseTenths { get; }
    }
}
