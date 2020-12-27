using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using UAlbion.Core.Visual;
using Veldrid;
using Veldrid.Utilities;

#pragma warning disable CA2213 // Analysis doesn't know about DisposeCollector
namespace UAlbion.Core.Veldrid
{
    public sealed class SceneContext : IDisposable
    {
        readonly DisposeCollector _disposer = new DisposeCollector();

        public DeviceBuffer IdentityMatrixBuffer { get; private set; }
        public DeviceBuffer ProjectionMatrixBuffer { get; private set; }
        public DeviceBuffer ModelViewMatrixBuffer { get; private set; }
        public DeviceBuffer DepthLimitsBuffer { get; internal set; }
        public DeviceBuffer CameraInfoBuffer { get; private set; }
        public ResourceSet CommonResourceSet { get; private set; }
        public ResourceLayout CommonResourceLayout { get; private set; }

        public Texture PaletteTexture { get; internal set; }
        public TextureView PaletteView { get; internal set; }
        public ICamera Camera { get; set; }
        public TextureSampleCount MainSceneSampleCount { get; internal set; }

        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            if (cl == null) throw new ArgumentNullException(nameof(cl));
            var factory = new DisposeCollectorResourceFactory(gd.ResourceFactory, _disposer);
            DeviceBuffer MakeBuffer(uint size, string name)
            {
                var buffer = factory.CreateBuffer(
                    new BufferDescription(size, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
                buffer.Name = name;
                return buffer;
            }

            ProjectionMatrixBuffer = MakeBuffer(64, "M_Projection");
            ModelViewMatrixBuffer = MakeBuffer(64, "M_View");
            IdentityMatrixBuffer = MakeBuffer(64, "M_Id");
            DepthLimitsBuffer = MakeBuffer((uint)Unsafe.SizeOf<DepthCascadeLimits>(), "B_DepthLimits");
            CameraInfoBuffer = MakeBuffer((uint)Unsafe.SizeOf<CameraInfo>(), "B_CameraInfo");

            cl.UpdateBuffer(IdentityMatrixBuffer, 0, Matrix4x4.Identity);

            var commonLayoutDescription = new ResourceLayoutDescription(
                ResourceLayoutHelper.Uniform("vdspv_1_0"), // CameraInfo / common data buffer
                ResourceLayoutHelper.UniformV("vdspv_1_1"), // Perspective Matrix
                ResourceLayoutHelper.UniformV("vdspv_1_2"), // View Matrix
                ResourceLayoutHelper.Texture("vdspv_1_3")); // PaletteTexture

            CommonResourceLayout = factory.CreateResourceLayout(commonLayoutDescription);
            CommonResourceLayout.Name = "RL_Common";
        }

        public void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            if (cl == null) throw new ArgumentNullException(nameof(cl));
            CommonResourceSet?.Dispose();
            CommonResourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                CommonResourceLayout,
                CameraInfoBuffer,
                ProjectionMatrixBuffer,
                ModelViewMatrixBuffer,
                PaletteView));

            CommonResourceSet.Name = "RS_Common";

            cl.UpdateBuffer(ProjectionMatrixBuffer, 0, Camera.ProjectionMatrix);
            cl.UpdateBuffer(ModelViewMatrixBuffer, 0, Camera.ViewMatrix);
            cl.UpdateBuffer(CameraInfoBuffer, 0, Camera.GetCameraInfo());
        }

        public void SetCurrentScene(Scene scene)
        {
            if (scene == null) throw new ArgumentNullException(nameof(scene));
            Camera = scene.Camera;
        }

        public void Dispose()
        {
            _disposer.DisposeAll();
            PaletteView?.Dispose();
            PaletteTexture?.Dispose();
            CommonResourceSet?.Dispose();

            PaletteView = null;
            PaletteTexture = null;
            CommonResourceSet = null;
        }
    }
}
#pragma warning restore CA2213 // Analysis doesn't know about DisposeCollector
