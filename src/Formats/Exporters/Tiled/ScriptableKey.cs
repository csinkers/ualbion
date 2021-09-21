namespace UAlbion.Formats.Exporters.Tiled
{
    readonly struct ScriptableKey
    {
        public ScriptableKey(ChainHint chainHint, byte[] eventBytes)
        {
            ChainHint = chainHint;
            EventBytes = eventBytes;
        }

        public ChainHint ChainHint { get; }
        public byte[] EventBytes { get; }
    }
}