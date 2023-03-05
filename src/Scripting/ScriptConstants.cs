using System;
using UAlbion.Api.Eventing;

namespace UAlbion.Scripting;

public static class ScriptConstants
{
    public const string FixedEventPrefix = "Event";
    public const string ChainPrefix = "Chain";
    public const string DummyLabelPrefix = "L_";

    public static string BuildDummyLabel(Guid id) => $"{DummyLabelPrefix}{id:N}";
    public static string BuildChainLabel(int chainNumber) => ChainPrefix + chainNumber;
    public static string BuildAdditionalEntryLabel(ushort eventId) => FixedEventPrefix + eventId;

    public static bool IsDummyLabel(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        return name.StartsWith(DummyLabelPrefix, StringComparison.Ordinal) &&
               Guid.TryParse(name[DummyLabelPrefix.Length..], out _);
    }

    public static (bool isChain, ushort id) ParseEntryPoint(string name)
    {
        if (string.IsNullOrEmpty(name))
            return (false, EventNode.UnusedEventId);

        if (name.StartsWith(ChainPrefix, StringComparison.OrdinalIgnoreCase))
            return (true, ushort.Parse(name[ChainPrefix.Length..]));

        if (name.StartsWith(FixedEventPrefix, StringComparison.OrdinalIgnoreCase))
            return (false, ushort.Parse(name[FixedEventPrefix.Length..]));

        return (false, EventNode.UnusedEventId);
    }
}
