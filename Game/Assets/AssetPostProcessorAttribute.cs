using System;

namespace UAlbion.Game.Assets
{
    public class AssetPostProcessorAttribute : Attribute
    {
        public AssetPostProcessorAttribute(params Type[] types) { Types = types; }

        public Type[] Types { get; }
    }
}