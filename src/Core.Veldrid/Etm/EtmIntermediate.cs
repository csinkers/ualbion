using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Etm
{
    [SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
    partial class EtmIntermediate : IVertexFormat
    {
#pragma warning disable 649
        [Vertex("TexCoords")] public Vector2 TextureCordinates;
        [Vertex("Textures", Flat = true)] public uint Textures;
        [Vertex("Flags", EnumPrefix = "TF", Flat = true)] public DungeonTileFlags Flags;
#pragma warning restore 649
    }
}