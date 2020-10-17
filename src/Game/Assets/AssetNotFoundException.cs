using System;
using UAlbion.Config;

namespace UAlbion.Game.Assets
{
    public sealed class AssetNotFoundException : Exception
    {
        public AssetType AssetType { get; }
        public int Id { get; }

        public AssetNotFoundException(string message, AssetType type, int id) : base(message)
        {
            AssetType = type;
            Id = id;
        }

        public AssetNotFoundException() { }
        public AssetNotFoundException(string message) : base(message) { }
        public AssetNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
