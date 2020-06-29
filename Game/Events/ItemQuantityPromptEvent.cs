using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("prompt:item_quantity")]
    public class ItemQuantityPromptEvent : IAsyncEvent<int>
    {
        public ItemQuantityPromptEvent(
            AssetType stringType, ushort stringId, int stringSubId,
            AssetType iconType, ushort iconId,
            int max, 
            bool useTenths) : this(new StringId(stringType, stringId, stringSubId), new AssetId(iconType, iconId), max, useTenths) { }

        public ItemQuantityPromptEvent(StringId text, AssetId icon, int max, bool useTenths)
        {
            Text = text;
            Icon = icon;
            Max = max;
            UseTenths = useTenths;
        }

        public StringId Text { get; }
        public AssetId Icon { get; }
        public int Max { get; }
        public bool UseTenths { get; }

        [EventPart("string_type")] public AssetType StringType => Text.Type;
        [EventPart("string_id")] public ushort StringId => Text.Id;
        [EventPart("string_sub_id")] public int StringSubId => Text.SubId;
        [EventPart("icon_type")] public AssetType IconType => Icon.Type;
        [EventPart("icon_id")] public ushort IconId => Icon.Id;
    }
}