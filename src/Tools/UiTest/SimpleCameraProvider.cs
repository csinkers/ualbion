using UAlbion.Core.Visual;

namespace UiTest;

public class SimpleCameraProvider : ICameraProvider
{
    public SimpleCameraProvider(ICamera camera) => Camera = camera ?? throw new ArgumentNullException(nameof(camera));
    public ICamera Camera { get; }
}