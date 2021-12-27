using System;
using UAlbion.Api;

namespace UAlbion.Scripting
{
    public static class ScriptConstants
    {
        public const string FixedEventPrefix = "Event";
        public const string ChainPrefix = "Chain";
        public const string DummyLabelPrefix = "L_";

        public static string BuildDummyLabel(Guid guid) => $"{DummyLabelPrefix}{Guid.NewGuid():N}";
        public static string BuildChainLabel(int chainNumber) => ChainPrefix + chainNumber;
        public static string BuildAdditionalEntryLabel(ushort eventId) => FixedEventPrefix + eventId;

        public static bool IsDummyLabel(string name) =>
            name.StartsWith(DummyLabelPrefix) && 
            Guid.TryParse(name[DummyLabelPrefix.Length..], out _);

        public static (bool isChain, ushort id) ParseEntryPoint(string name)
        {
            if (string.IsNullOrEmpty(name))
                return (false, EventNode.UnusedEventId);

            if (name.StartsWith(ChainPrefix))
                return (true, ushort.Parse(name[ChainPrefix.Length..]));

            if (name.StartsWith(FixedEventPrefix))
                return (false, ushort.Parse(name[FixedEventPrefix.Length..]));

            return (false, EventNode.UnusedEventId);
        }
    }
}