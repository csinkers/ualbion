using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid.Diag;

public interface IImGuiWindow : IComponent
{
    string Name { get; }
    ImGuiWindowDrawResult Draw();
}