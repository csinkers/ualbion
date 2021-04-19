namespace UAlbion.Core.Visual
{
    public interface IFramebufferSource : IVisualComponent
    {
        uint Width { get; set; }
        uint Height { get; set; }
    }
}