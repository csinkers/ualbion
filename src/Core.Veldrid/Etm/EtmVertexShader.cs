using System.Diagnostics.CodeAnalysis;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Etm
{
    [Name("ExtrudedTileMapSV.vert")]
    [Input(0, typeof(Vertex3DTextured))]
    [Input(1, typeof(DungeonTile), InstanceStep = 1)]
    [ResourceSet(0, typeof(EtmSet))]
    [ResourceSet(1, typeof(CommonSet))]
    [Output(0, typeof(EtmIntermediate))]
    [SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
    partial class EtmVertexShader : IVertexShader { }
}