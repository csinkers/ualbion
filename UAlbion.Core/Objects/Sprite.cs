using System;
using System.Numerics;
using Veldrid;

namespace UAlbion.Core.Objects
{
    public class Sprite : Renderable
    {
        static Vertex2DTextured Vertex(float x, float y, float u, float v) => new Vertex2DTextured(new Vector2(x, y), new Vector2(u, v));
        readonly SpriteRenderer _renderer;
        Texture _deviceTexture;
        TextureView _textureView;

        public Sprite(SpriteRenderer renderer, ITexture texture)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            Size = new Vector2(texture.Width, texture.Height);
        }

        public ITexture Texture { get; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public SpriteFlags Flags { get; set; }
        public Vector2 TexPosition { get; } = new Vector2(0.0f, 0.0f);
        public Vector2 TexSize { get; } = new Vector2(1.0f, 1.0f);
        public ResourceSet ResourceSet { get; private set; }

        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            _renderer.UpdatePerFrameResources(gd, cl, sc);
            if (_deviceTexture == null)
            {
                _deviceTexture = Texture.CreateDeviceTexture(gd, gd.ResourceFactory, TextureUsage.Sampled);
                _textureView = gd.ResourceFactory.CreateTextureView(_deviceTexture);
            }
        }

        public override void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            _renderer.Render(gd, cl, sc, renderPass, this);
        }

        public override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            _renderer.CreateDeviceObjects(gd, cl, sc);
            ResourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_renderer.ResourceLayout,
                sc.ProjectionMatrixBuffer, sc.ViewMatrixBuffer,
                _textureView, gd.PointSampler));
        }

        public override void DestroyDeviceObjects()
        {
            _textureView?.Dispose();
            _deviceTexture?.Dispose();
            _renderer.DestroyDeviceObjects();
        }

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition) => _renderer.GetRenderOrderKey(cameraPosition);
    }
}