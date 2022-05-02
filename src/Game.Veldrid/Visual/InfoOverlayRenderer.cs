using UAlbion.Api.Eventing;
using UAlbion.Core;

namespace UAlbion.Game.Veldrid.Visual;

public sealed class InfoOverlayRenderer : Component
{
    /*
    IPipelineHolder _pipeline;

    public void Render(CommandList cl, InfoOverlay overlay, CommonSet commonSet)
    {
        cl.PushDebugGroup(overlay.Name);

        cl.SetPipeline(_pipeline.Pipeline);
        cl.SetGraphicsResourceSet(0, overlay.ResourceSet.ResourceSet);
        cl.SetGraphicsResourceSet(1, commonSet.ResourceSet);
        cl.SetVertexBuffer(0, _vertexBuffer);
        cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        cl.DrawIndexed((uint)Indices.Length);
        cl.PopDebugGroup();
    }

    public void Dispose() { }
    */
}