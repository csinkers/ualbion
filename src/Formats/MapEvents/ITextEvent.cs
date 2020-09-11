using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public interface ITextEvent : IEvent
    {
        AssetType TextType { get; }
        ushort TextSourceId { get; }
    }
}
