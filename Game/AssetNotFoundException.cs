using System;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game
{
    class AssetNotFoundException : Exception
    {
        public AssetType Type { get; }
        public int Id { get; }

        public AssetNotFoundException(string message, AssetType type, int id) : base(message)
        {
            Type = type;
            Id = id;
        }
    }
}