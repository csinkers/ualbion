using System;
using System.Numerics;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.Utilities;

namespace UAlbion.Core.Objects
{
    public class TexturedMesh : CullRenderable
    {
        // Useful for testing uniform bindings with an offset.
        static readonly bool s_useUniformOffset = false;
        uint _uniformOffset = 0;

        readonly string _name;
        readonly MeshData _meshData;
        readonly ImageSharpTexture _textureData;
        readonly ImageSharpTexture _alphaTextureData;
        readonly Transform _transform = new Transform();

        readonly BoundingBox _centeredBounds;
        DeviceBuffer _vb;
        DeviceBuffer _ib;
        int _indexCount;
        Texture _texture;
        Texture _alphamapTexture;
        TextureView _alphaMapView;

        Pipeline _pipeline;
        ResourceSet _mainProjViewRS;
        ResourceSet _mainSharedRS;
        ResourceSet _mainPerObjectRS;

        DeviceBuffer _worldAndInverseBuffer;

        readonly DisposeCollector _disposeCollector = new DisposeCollector();

        readonly MaterialPropsAndBuffer _materialProps;
        readonly Vector3 _objectCenter;
        readonly bool _materialPropsOwned = false;

        public Transform Transform => _transform;

        public TexturedMesh(string name, MeshData meshData, ImageSharpTexture textureData, ImageSharpTexture alphaTexture, MaterialPropsAndBuffer materialProps)
        {
            _name = name;
            _meshData = meshData;
            _centeredBounds = meshData.GetBoundingBox();
            _objectCenter = _centeredBounds.GetCenter();
            _textureData = textureData;
            _alphaTextureData = alphaTexture;
            _materialProps = materialProps;
        }

        public override BoundingBox BoundingBox => BoundingBox.Transform(_centeredBounds, _transform.GetTransformMatrix());

        public override unsafe void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            if (s_useUniformOffset)
            {
                _uniformOffset = gd.UniformBufferMinOffsetAlignment;
            }
            ResourceFactory disposeFactory = new DisposeCollectorResourceFactory(gd.ResourceFactory, _disposeCollector);
            _vb = _meshData.CreateVertexBuffer(disposeFactory, cl);
            _vb.Name = _name + "_VB";
            _ib = _meshData.CreateIndexBuffer(disposeFactory, cl, out _indexCount);
            _ib.Name = _name + "_IB";

            uint bufferSize = 128;
            if (s_useUniformOffset) { bufferSize += _uniformOffset * 2; }

            _worldAndInverseBuffer = disposeFactory.CreateBuffer(new BufferDescription(bufferSize, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            if (_materialPropsOwned)
            {
                _materialProps.CreateDeviceObjects(gd, cl, sc);
            }

            if (_textureData != null)
            {
                _texture = StaticResourceCache.GetTexture2D(gd, gd.ResourceFactory, _textureData);
            }
            else
            {
                _texture = disposeFactory.CreateTexture(TextureDescription.Texture2D(1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
                RgbaByte color = RgbaByte.Pink;
                gd.UpdateTexture(_texture, (IntPtr)(&color), 4, 0, 0, 0, 1, 1, 1, 0, 0);
            }

            if (_alphaTextureData != null)
            {
                _alphamapTexture = _alphaTextureData.CreateDeviceTexture(gd, disposeFactory);
            }
            else
            {
                _alphamapTexture = StaticResourceCache.GetPinkTexture(gd, gd.ResourceFactory);
            }
            _alphaMapView = StaticResourceCache.GetTextureView(gd.ResourceFactory, _alphamapTexture);

            VertexLayoutDescription[] shadowDepthVertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
            };

            (Shader depthVS, Shader depthFS) = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, "ShadowDepth");

            ResourceLayout projViewCombinedLayout = StaticResourceCache.GetResourceLayout(
                gd.ResourceFactory,
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("ViewProjection", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            ResourceLayout worldLayout = StaticResourceCache.GetResourceLayout(gd.ResourceFactory, new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("WorldAndInverse", ResourceKind.UniformBuffer, ShaderStages.Vertex, ResourceLayoutElementOptions.DynamicBinding)));

            VertexLayoutDescription[] mainVertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
            };

            (Shader mainVS, Shader mainFS) = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, "ShadowMain");

            ResourceLayout projViewLayout = StaticResourceCache.GetResourceLayout(
                gd.ResourceFactory,
                StaticResourceCache.ProjViewLayoutDescription);

            ResourceLayout mainSharedLayout = StaticResourceCache.GetResourceLayout(gd.ResourceFactory, new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("LightViewProjection1", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("LightViewProjection2", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("LightViewProjection3", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("DepthLimits", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("LightInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("PointLights", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)));

            ResourceLayout mainPerObjectLayout = StaticResourceCache.GetResourceLayout(gd.ResourceFactory, new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("WorldAndInverse", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment, ResourceLayoutElementOptions.DynamicBinding),
                new ResourceLayoutElementDescription("MaterialProperties", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("RegularSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("AlphaMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("AlphaMapSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapNear", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapMid", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapFar", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            ResourceLayout reflectionLayout = StaticResourceCache.GetResourceLayout(gd.ResourceFactory, new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ReflectionMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ReflectionSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ReflectionViewProj", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("ClipPlaneInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

            BlendStateDescription alphaBlendDesc = BlendStateDescription.SingleAlphaBlend;
            // alphaBlendDesc.AlphaToCoverageEnabled = true;

            GraphicsPipelineDescription mainPD = new GraphicsPipelineDescription(
                _alphamapTexture != null ? alphaBlendDesc : BlendStateDescription.SingleOverrideBlend,
                gd.IsDepthRangeZeroToOne ? DepthStencilStateDescription.DepthOnlyGreaterEqual : DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(mainVertexLayouts, new[] { mainVS, mainFS }, new[] { new SpecializationConstant(100, gd.IsClipSpaceYInverted) }),
                new ResourceLayout[] { projViewLayout, mainSharedLayout, mainPerObjectLayout, reflectionLayout },
                sc.MainSceneFramebuffer.OutputDescription);
            _pipeline = StaticResourceCache.GetPipeline(gd.ResourceFactory, ref mainPD);
            _pipeline.Name = "TexturedMesh Main Pipeline";

            _mainProjViewRS = StaticResourceCache.GetResourceSet(gd.ResourceFactory, new ResourceSetDescription(projViewLayout,
                sc.ProjectionMatrixBuffer,
                sc.ViewMatrixBuffer));

            _mainSharedRS = StaticResourceCache.GetResourceSet(gd.ResourceFactory, new ResourceSetDescription(mainSharedLayout,
                sc.LightViewProjectionBuffer0,
                sc.LightViewProjectionBuffer1,
                sc.LightViewProjectionBuffer2,
                sc.DepthLimitsBuffer,
                sc.LightInfoBuffer,
                sc.CameraInfoBuffer,
                sc.PointLightsBuffer));

            _mainPerObjectRS = disposeFactory.CreateResourceSet(new ResourceSetDescription(mainPerObjectLayout,
                new DeviceBufferRange(_worldAndInverseBuffer, _uniformOffset, 128),
                _materialProps.UniformBuffer,
                _texture,
                gd.Aniso4xSampler,
                _alphaMapView,
                gd.LinearSampler,
                gd.PointSampler));

        }

        public override void DestroyDeviceObjects()
        {
            if (_materialPropsOwned)
            {
                _materialProps.DestroyDeviceObjects();
            }

            _disposeCollector.DisposeAll();
        }

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return RenderOrderKey.Create(
                _pipeline.GetHashCode(),
                Vector3.Distance((_objectCenter * _transform.Scale) + _transform.Position, cameraPosition));
        }

        public override RenderPasses RenderPasses => _alphaTextureData != null ? RenderPasses.AlphaBlend : RenderPasses.Standard;

        public override void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            if (_materialPropsOwned)
            {
                _materialProps.FlushChanges(cl);
            }

            if (renderPass == RenderPasses.Standard || renderPass == RenderPasses.AlphaBlend)
            {
                RenderStandard(cl, sc, false);
            }
        }

        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            WorldAndInverse wai;
            wai.World = _transform.GetTransformMatrix();
            wai.InverseWorld = VdUtilities.CalculateInverseTranspose(ref wai.World);
            gd.UpdateBuffer(_worldAndInverseBuffer, _uniformOffset * 2, ref wai);
        }

        void RenderStandard(CommandList cl, SceneContext sc, bool reflectionPass)
        {
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, _mainProjViewRS);
            cl.SetGraphicsResourceSet(1, _mainSharedRS);
            uint offset = _uniformOffset;
            cl.SetGraphicsResourceSet(2, _mainPerObjectRS, 1, ref offset);
            cl.DrawIndexed((uint)_indexCount, 1, 0, 0, 0);
        }
    }

    public struct WorldAndInverse
    {
        public Matrix4x4 World;
        public Matrix4x4 InverseWorld;
    }
}
