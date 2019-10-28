using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public static class AssetNameResolver
    {
        static readonly IReadOnlyDictionary<AssetType, Type> IdTypes =
            Enum.GetValues(typeof(AssetType)).Cast<AssetType>().Select(x =>
            {
                var members = typeof(AssetType).GetMember(x.ToString());
                var member = members.FirstOrDefault(m => m.DeclaringType == typeof(AssetType));
                var attributes = member?.GetCustomAttributes(typeof(EnumTypeAttribute), false);
                if (attributes?.Any() ?? false)
                    return (x, ((EnumTypeAttribute) attributes[0]).EnumType);

                return (x, null);
            }).Where(x => x.EnumType != null).ToDictionary(x => x.x, x => x.EnumType);


        public static string GetName(AssetType type, int id) => IdTypes.TryGetValue(type, out var enumType) ? Enum.GetName(enumType, id) : id.ToString();
    }
}