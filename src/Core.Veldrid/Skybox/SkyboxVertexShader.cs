using System.Diagnostics.CodeAnalysis;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Skybox;

[Name("SkyBoxSV.vert")]
[Input(0, typeof(Vertex2DTextured))]
[ResourceSet(0, typeof(SkyboxResourceSet))]
[Output(0, typeof(SkyboxIntermediate))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
internal partial class SkyboxVertexShader : IVertexShader { }