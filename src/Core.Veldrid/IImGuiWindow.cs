using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid;

public interface IImGuiWindow : IComponent
{
    string Name { get; }
    void Draw();
}