using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using Veldrid;
using Veldrid.Utilities;

#pragma warning disable CA2213 // Analysis doesn't know about DisposeCollector
namespace UAlbion.Core.Veldrid
{
    public sealed class SceneContext : Component, IDisposable
    {
        readonly DisposeCollector _disposer = new DisposeCollector();
        IReadOnlyTexture<uint> _paletteTexture;
        int _lastPaletteVersion = -1;

        public DeviceBuffer IdentityMatrixBuffer { get; private set; }
        public DeviceBuffer ProjectionMatrixBuffer { get; private set; }
        public DeviceBuffer ModelViewMatrixBuffer { get; private set; }
        public DeviceBuffer DepthLimitsBuffer { get; private set; }
        public DeviceBuffer CameraInfoBuffer { get; private set; }
        public ResourceSet CommonResourceSet { get; private set; }
        public ResourceLayout CommonResourceLayout { get; private set; }
        public Texture PaletteTexture { get; private set; }
        public TextureView PaletteView { get; private set; }
        public TextureSampleCount MainSceneSampleCount { get; private set; } // TODO: Implement

        public SceneContext()
        {
            On<SetMsaaLevelEvent>(e => {
                var sampleCount = e.SampleCount switch
                {
                    1 => TextureSampleCount.Count1,
                    2 => TextureSampleCount.Count2,
                    4 => TextureSampleCount.Count4,
                    8 => TextureSampleCount.Count8,
                    16 => TextureSampleCount.Count16,
                    32 => TextureSampleCount.Count32,
                    _ => throw new InvalidOperationException($"Invalid sample count {e.SampleCount}")
                };

                MainSceneSampleCount = sampleCount;
            });
        }

        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            if (cl == null) throw new ArgumentNullException(nameof(cl));
            var factory = new DisposeCollectorResourceFactory(gd.ResourceFactory, _disposer);
            DeviceBuffer MakeBuffer(uint size, string name)
            {
                var buffer = factory.CreateBuffer(new BufferDescription(size, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
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
                ResourceLayoutHelper.Uniform("_Shared"), // CameraInfo / common data buffer
                ResourceLayoutHelper.UniformV("_Projection"), // Perspective Matrix
                ResourceLayoutHelper.UniformV("_View"), // View Matrix
                ResourceLayoutHelper.Texture("uPalette")); // PaletteTexture

            CommonResourceLayout = factory.CreateResourceLayout(commonLayoutDescription);
            CommonResourceLayout.Name = "RL_Common";

            var paletteManager = Resolve<IPaletteManager>();
            SetCurrentPalette(gd, paletteManager);
            UpdateResourceSet(gd);
        }

        public void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, IFramebufferSource framebuffer)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            if (cl == null) throw new ArgumentNullException(nameof(cl));

            var camera = Resolve<ICamera>();
            var clock = TryResolve<IClock>();
            var settings = TryResolve<IEngineSettings>();
            var paletteManager = Resolve<IPaletteManager>();
            var window = Resolve<IWindowManager>();

            SetCurrentPalette(gd, paletteManager);

            var info = new CameraInfo
            {
                WorldSpacePosition = camera.Position,
                CameraPitch = camera.Pitch,
                CameraYaw = camera.Yaw,
                Resolution =  framebuffer != null
                    ? new Vector2(framebuffer.Width, framebuffer.Height)
                    : new Vector2(window.PixelWidth, window.PixelHeight),
                Time = clock?.ElapsedTime ?? 0,
                Special1 = settings?.Special1 ?? 0,
                Special2 = settings?.Special2 ?? 0,
                EngineFlags = (uint?)settings?.Flags ?? 0,
                PaletteBlend = paletteManager.PaletteBlend
            };

            cl.UpdateBuffer(ProjectionMatrixBuffer, 0, camera.ProjectionMatrix);
            cl.UpdateBuffer(ModelViewMatrixBuffer, 0, camera.ViewMatrix);
            cl.UpdateBuffer(CameraInfoBuffer, 0, info);
        }

        void UpdateResourceSet(GraphicsDevice gd)
        {
            CommonResourceSet?.Dispose();
            CommonResourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                CommonResourceLayout,
                CameraInfoBuffer,
                ProjectionMatrixBuffer,
                ModelViewMatrixBuffer,
                PaletteView));

            CommonResourceSet.Name = "RS_Common";
        }

        void SetCurrentPalette(GraphicsDevice gd, IPaletteManager paletteManager)
        {
            var newPalette = paletteManager.PaletteTexture;
            int newVersion = paletteManager.Version;

            if (newPalette == null) return;
            if (PaletteView != null && _paletteTexture == newPalette && _lastPaletteVersion == newVersion)
                return;

            PaletteView?.Dispose();
            PaletteTexture?.Dispose();
            CoreTrace.Log.Info("Scene", "Disposed palette device texture");
            _paletteTexture = newPalette;
            _lastPaletteVersion = newVersion;
            PaletteTexture = VeldridTexture.CreateDeviceTexture(gd, TextureUsage.Sampled, _paletteTexture);
            PaletteView = gd.ResourceFactory.CreateTextureView(PaletteTexture);
            PaletteTexture.Name = "T_" + _paletteTexture.Name;
            PaletteView.Name = "TV_" + _paletteTexture.Name;
            UpdateResourceSet(gd);
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
