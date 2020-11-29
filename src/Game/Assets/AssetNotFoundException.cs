using System;
using UAlbion.Config;

namespace UAlbion.Game.Assets
{
    public sealed class AssetNotFoundException : Exception
    {
        public AssetId Id { get; }
        public AssetNotFoundException(string message, AssetId id) : base(message) => Id = id;
        public AssetNotFoundException() { }
        public AssetNotFoundException(string message) : base(message) { }
        public AssetNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
