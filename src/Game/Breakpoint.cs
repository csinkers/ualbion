using System.Text;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game;

public record Breakpoint(
    TriggerType? TriggerType,
    AssetId Target,
    // Type EventType,
    int? EventId
)
{
    public override string ToString()
    {
        var sb = new StringBuilder();
        if (TriggerType.HasValue)
        {
            sb.Append(TriggerType.Value);
            sb.Append(' ');
        }

        if (!Target.IsNone)
        {
            sb.Append(Target);
            sb.Append(' ');
        }
/*
        if (EventType != null)
        {
            sb.Append(EventType);
            sb.Append(' ');
        }
*/

        if (EventId.HasValue)
        {
            sb.Append(EventId.Value);
            sb.Append(' ');
        }

        return sb.ToString();
    }
}