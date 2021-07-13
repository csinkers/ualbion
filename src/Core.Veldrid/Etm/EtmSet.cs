using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Etm
{
    partial class EtmSet : ResourceSetHolder
    {
        [Resource("Properties", ShaderStages.Vertex)] IBufferHolder<DungeonTileMapProperties> _properties;
        [Resource("DayFloors", ShaderStages.Fragment)] ITextureArrayHolder _dayFloors;
        [Resource("DayWalls", ShaderStages.Fragment)] ITextureArrayHolder _dayWalls;
        [Resource("NightFloors", ShaderStages.Fragment)] ITextureArrayHolder _nightFloors;
        [Resource("NightWalls", ShaderStages.Fragment)] ITextureArrayHolder _nightWalls;
        [Resource("TextureSampler", ShaderStages.Fragment)] ISamplerHolder _textureSampler;
    }
}