using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public interface IResourceProvider
{
    IResourceSetHolder ResourceSet { get; }
}