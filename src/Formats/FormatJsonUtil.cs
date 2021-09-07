using System;
using System.Linq;
using System.Text.Json.Serialization;
using UAlbion.Api;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats
{
    public class FormatJsonUtil : JsonUtil
    {
        public FormatJsonUtil(params JsonConverter[] extraConverters)
            : base(new[] {
                (JsonConverter)EventNodeConverter.Instance,
                InventoryConverter.Instance,
                NpcWaypointConverter.Instance, 
            }.Concat(extraConverters ?? Array.Empty<JsonConverter>()).ToArray())
        {
        }
    }
}