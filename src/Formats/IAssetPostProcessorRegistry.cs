using System;
using UAlbion.Config;

namespace UAlbion.Formats;

public interface IAssetPostProcessorRegistry
{
    IAssetPostProcessor GetPostProcessor(Type postProcessorType);
}