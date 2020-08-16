using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;
using Veldrid.Utilities;

#pragma warning disable CA2213 // Analysis doesn't know about DisposeCollector
namespace UAlbion.Core.Veldrid
{
    public sealed class SceneContext : IDisposable
    {
        readonly DisposeCollector _disposer = new DisposeCollector();
        WindowSizedSceneContext _windowSized;

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

        public ResourceLayout TextureSamplerResourceLayout => _windowSized.TextureSamplerResourceLayout;
        public Texture MainSceneColorTexture => _windowSized.MainSceneColorTexture;
        public Texture MainSceneDepthTexture => _windowSized.MainSceneDepthTexture;
        public Framebuffer MainSceneFramebuffer => _windowSized.MainSceneFramebuffer;
        public Texture MainSceneResolvedColorTexture => _windowSized.MainSceneResolvedColorTexture;
        public TextureView MainSceneResolvedColorView => _windowSized.MainSceneResolvedColorView;
        public ResourceSet MainSceneViewResourceSet => _windowSized.MainSceneViewResourceSet;

        public Texture DuplicatorTarget0 => _windowSized.DuplicatorTarget0;
        public TextureView DuplicatorTargetView0 => _windowSized.DuplicatorTargetView0;
        public ResourceSet DuplicatorTargetSet0 => _windowSized.DuplicatorTargetSet0;
        public Framebuffer DuplicatorFramebuffer => _windowSized.DuplicatorFramebuffer;

        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            if (cl == null) throw new ArgumentNullException(nameof(cl));
            var factory = new DisposingResourceFactoryFacade(gd.ResourceFactory, _disposer);
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

            _windowSized = new WindowSizedSceneContext(gd, MainSceneSampleCount);
            _disposer.Add(_windowSized, CommonResourceLayout);
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

        public void DestroyDeviceObjects()
        {
            _disposer.DisposeAll();
            PaletteView?.Dispose();
            PaletteTexture?.Dispose();
            CommonResourceSet?.Dispose();

            PaletteView = null;
            PaletteTexture = null;
            CommonResourceSet = null;
        }

        public void SetCurrentScene(Scene scene)
        {
            if (scene == null) throw new ArgumentNullException(nameof(scene));
            Camera = scene.Camera;
        }

        public void RecreateWindowSizedResources(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException(nameof(graphicsDevice));
            _disposer.Remove(_windowSized);
            _windowSized.Dispose();
            _windowSized = new WindowSizedSceneContext(graphicsDevice, MainSceneSampleCount);
            _disposer.Add(_windowSized);
        }

        public void Dispose() => DestroyDeviceObjects();
    }
}
#pragma warning restore CA2213 // Analysis doesn't know about DisposeCollector
