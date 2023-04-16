using Veldrid;

namespace UAlbion.Core.Veldrid;

public interface IVeldridEngine : IEngine
{
    GraphicsDevice Device { get; }
}