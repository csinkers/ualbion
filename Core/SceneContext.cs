using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Core
{
    public class SceneContext
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

        public virtual void CreateDeviceObjects(GraphicsDevice gd, CommandList cl)
        {
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

            if (Camera != null)
                UpdateCameraBuffers(cl);

            var commonLayoutDescription = new ResourceLayoutDescription(
                ResourceLayoutHelper.Uniform("vdspv_1_0")); // CameraInfo / common data buffer
            CommonResourceLayout = factory.CreateResourceLayout(commonLayoutDescription);
            CommonResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                CommonResourceLayout,
                CameraInfoBuffer));
            CommonResourceLayout.Name = "RL_Common";
            CommonResourceSet.Name = "RS_Common";

            _windowSized = new WindowSizedSceneContext(gd, MainSceneSampleCount);
            _disposer.Add(_windowSized);
        }

        public virtual void DestroyDeviceObjects()
        {
            _disposer.DisposeAll();
            PaletteView?.Dispose();
            PaletteTexture?.Dispose();

            PaletteView = null;
            PaletteTexture = null;
        }

        public void SetCurrentScene(Scene scene)
        {
            Camera = scene.Camera;
        }

        public void UpdateCameraBuffers(CommandList cl)
        {
            cl.UpdateBuffer(ProjectionMatrixBuffer, 0, Camera.ProjectionMatrix);
            cl.UpdateBuffer(CameraInfoBuffer, 0, Camera.GetCameraInfo());
        }

        public void UpdateModelTransform(CommandList cl, Matrix4x4 transform)
        {
            cl.UpdateBuffer(ModelViewMatrixBuffer, 0, transform);
        }

        public void RecreateWindowSizedResources(GraphicsDevice graphicsDevice)
        {
            _disposer.Remove(_windowSized);
            _windowSized.Dispose();
            _windowSized = new WindowSizedSceneContext(graphicsDevice, MainSceneSampleCount);
            _disposer.Add(_windowSized);
        }
    }
}
