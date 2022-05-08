using System.Diagnostics.CodeAnalysis;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites;

[Name("BlendedSpriteSV.vert")]
[Input(0, typeof(Vertex2DTextured))]
[Input(1, typeof(GpuBlendedSpriteInstanceData), InstanceStep = 1)]
[ResourceSet(0, typeof(CommonSet))]
[ResourceSet(1, typeof(SpriteSet))]
[Output(0, typeof(BlendedSpriteIntermediateData))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
internal partial class BlendedSpriteVertexShader : IVertexShader { }