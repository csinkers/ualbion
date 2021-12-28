using System.Diagnostics.CodeAnalysis;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Skybox;

[Name("SkyBoxSF.frag")]
[Input(0, typeof(SkyboxIntermediate))]
[ResourceSet(0, typeof(SkyboxResourceSet))]
[ResourceSet(1, typeof(CommonSet))]
[Output(0, typeof(SimpleFramebuffer))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
internal partial class SkyboxFragmentShader : IFragmentShader { }