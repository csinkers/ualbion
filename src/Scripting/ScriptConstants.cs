using System;

namespace UAlbion.Scripting
{
    public static class ScriptConstants
    {
        public const string FixedEventPrefix = "Event";
        public const string ChainPrefix = "Chain";
        public const string DummyLabelPrefix = "L_";

        public static string BuildDummyLabel(Guid guid) => $"{DummyLabelPrefix}{Guid.NewGuid():N}";
        public static string BuildChainLabel(int chainNumber) => $"Chain{chainNumber}";
        public static string BuildAdditionalEntryLabel(ushort eventId) => $"Event{eventId}";

        public static bool IsDummyLabel(string name) =>
            name.StartsWith(DummyLabelPrefix) && 
            Guid.TryParse(name[DummyLabelPrefix.Length..], out _);
    }
}