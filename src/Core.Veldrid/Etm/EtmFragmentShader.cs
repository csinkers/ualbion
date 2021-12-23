using System.Diagnostics.CodeAnalysis;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Etm
{
    [Name( "ExtrudedTileMapSF.frag")]
    [Input(0, typeof(EtmIntermediate))]
    [ResourceSet(0, typeof(EtmSet))]
    [ResourceSet(1, typeof(CommonSet))]
    [Output(0, typeof(SimpleFramebuffer))]
    [SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
    partial class EtmFragmentShader : IFragmentShader { }
}