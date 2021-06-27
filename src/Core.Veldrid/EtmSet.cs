using UAlbion.Core.Veldrid.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    partial class EtmSet : ResourceSetHolder
    {
        [Resource("Properties", ShaderStages.Vertex)] SingleBuffer<DungeonTileMapProperties> _properties;
        [Resource("DayFloors", ShaderStages.Fragment)] Texture2DArrayHolder _dayFloors;
        [Resource("DayWalls", ShaderStages.Fragment)] Texture2DArrayHolder _dayWalls;
        [Resource("NightFloors", ShaderStages.Fragment)] Texture2DArrayHolder _nightFloors;
        [Resource("NightWalls", ShaderStages.Fragment)] Texture2DArrayHolder _nightWalls;
        [Resource("TextureSampler", ShaderStages.Fragment)] SamplerHolder _textureSampler;
    }
}