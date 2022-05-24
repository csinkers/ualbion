using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

public class EventSource
{
    public EventSource(AssetId assetId, TextId textSource, TriggerTypes trigger, int x = 0, int y = 0)
    {
        // Trigger = TalkTo for NPC, UseItem for item, Action for event set, Default for none
        AssetId = assetId;
        TextSource = textSource;
        Trigger = trigger;
        X = x;
        Y = y;
    }

    public AssetId AssetId { get; }
    public TextId TextSource { get; }
    public TriggerTypes Trigger { get; }
    public int X { get; }
    public int Y { get; }
    public override string ToString() => $"ESrc({Trigger} {AssetId} @ ({X},{Y}) T:{TextSource})";
}