using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;

namespace UAlbion.Core
{
    public class Scene
    {
        static readonly Func<RenderPasses, Func<CullRenderable, bool>> s_createFilterFunc = CreateFilter;
        readonly Octree<CullRenderable> _octree = new Octree<CullRenderable>(
            new BoundingBox(Vector3.One * -50, Vector3.One * 50), 2);

        readonly List<Renderable> _freeRenderables = new List<Renderable>();

        readonly ConcurrentDictionary<RenderPasses, Func<CullRenderable, bool>> _filters
           = new ConcurrentDictionary<RenderPasses, Func<CullRenderable, bool>>(new RenderPassesComparer());

        readonly HashSet<Renderable> _allPerFrameRenderablesSet = new HashSet<Renderable>();
        readonly RenderQueue[] _renderQueues = Enumerable.Range(0, 4).Select(i => new RenderQueue()).ToArray();
        readonly List<CullRenderable>[] _cullableStage = Enumerable.Range(0, 4).Select(i => new List<CullRenderable>()).ToArray();
        readonly List<Renderable>[] _renderableStage = Enumerable.Range(0, 4).Select(i => new List<Renderable>()).ToArray();
        uint[] _palette;

        // internal MirrorMesh MirrorMesh { get; set; } = new MirrorMesh();

        ICamera _camera;
        CommandList _resourceUpdateCL;

        public ICamera Camera => _camera;

        public bool ThreadedRendering { get; set; } = false;
        public EventExchange Exchange { get; } = new EventExchange();

        public Scene(GraphicsDevice gd, Sdl2Window window, ICamera camera)
        {
            _camera = camera;
        }

        public void SetPalette(uint[] palette)
        {
            _palette = palette;
        }

        public void AddRenderable(Renderable r)
        {
            if (r is CullRenderable cr)
            {
                _octree.AddItem(cr.BoundingBox, cr);
            }
            else
            {
                _freeRenderables.Add(r);
            }
        }

        public void AddComponent(IComponent component)
        {
            Debug.Assert(component != null);
            component.Attach(Exchange);
        }

        public void RemoveRenderable(Renderable r)
        {
            throw new NotImplementedException();
        }

        public void RemoveComponent(IComponent component)
        {
            Exchange.Unsubscribe(component);
        }

        public void RenderAllStages(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            float depthClear = gd.IsDepthRangeZeroToOne ? 0f : 1f;

            // Main scene
            cl.PushDebugGroup("Main Scene Pass");
            cl.SetFramebuffer(sc.MainSceneFramebuffer);
            var fbWidth = sc.MainSceneFramebuffer.Width;
            var fbHeight = sc.MainSceneFramebuffer.Height;
            cl.SetViewport(0, new Viewport(0, 0, fbWidth, fbHeight, 0, 1));
            cl.SetFullViewports();
            cl.SetFullScissorRects();
            cl.ClearDepthStencil(depthClear);
            sc.UpdateCameraBuffers(cl); // Re-set because reflection step changed it.
            var cameraFrustum = new BoundingFrustum(_camera.ViewMatrix * _camera.ProjectionMatrix);
            Render(gd, cl, sc, RenderPasses.Standard, cameraFrustum, _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0]);
            cl.PopDebugGroup();

            //cl.PushDebugGroup("Transparent Pass");
            //Render(gd, cl, sc, RenderPasses.AlphaBlend, cameraFrustum, _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0]);
            //cl.PopDebugGroup();

            cl.PushDebugGroup("Overlay");
            Render(gd, cl, sc, RenderPasses.Overlay, cameraFrustum, _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0]);
            cl.PopDebugGroup();

            if (sc.MainSceneColorTexture.SampleCount != TextureSampleCount.Count1)
            {
                cl.ResolveTexture(sc.MainSceneColorTexture, sc.MainSceneResolvedColorTexture);
            }

            cl.PushDebugGroup("Duplicator");
            cl.SetFramebuffer(sc.DuplicatorFramebuffer);
            cl.SetFullViewports();
            Render(gd, cl, sc, RenderPasses.Duplicator, new BoundingFrustum(), _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0]);
            cl.PopDebugGroup();

            cl.PushDebugGroup("Swapchain Pass");
            cl.SetFramebuffer(gd.SwapchainFramebuffer);
            cl.SetFullViewports();
            Render(gd, cl, sc, RenderPasses.SwapchainOutput, new BoundingFrustum(), _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0]);
            cl.PopDebugGroup();

            cl.End();

            _resourceUpdateCL.Begin();
            foreach (Renderable renderable in _allPerFrameRenderablesSet)
            {
                renderable.UpdatePerFrameResources(gd, _resourceUpdateCL, sc);
            }
            _resourceUpdateCL.End();

            gd.SubmitCommands(_resourceUpdateCL);
            gd.SubmitCommands(cl);
        }

        public void Render(
            GraphicsDevice gd,
            CommandList rc,
            SceneContext sc,
            RenderPasses pass,
            BoundingFrustum frustum,
            Vector3 viewPosition,
            RenderQueue renderQueue,
            List<CullRenderable> cullRenderableList,
            List<Renderable> renderableList)
        {
            renderQueue.Clear();

            cullRenderableList.Clear();
            CollectVisibleObjects(ref frustum, pass, cullRenderableList);
            renderQueue.AddRange(cullRenderableList, viewPosition);

            renderableList.Clear();
            CollectFreeObjects(pass, renderableList);
            renderQueue.AddRange(renderableList, viewPosition);

            renderQueue.Sort();

            foreach (Renderable renderable in renderQueue) { renderable.Render(gd, rc, sc, pass); }
            foreach (CullRenderable thing in cullRenderableList) { _allPerFrameRenderablesSet.Add(thing); }
            foreach (Renderable thing in renderableList) { _allPerFrameRenderablesSet.Add(thing); }
        }

        void CollectVisibleObjects(
            ref BoundingFrustum frustum,
            RenderPasses renderPass,
            List<CullRenderable> renderables)
        {
            _octree.GetContainedObjects(frustum, renderables, GetFilter(renderPass));
        }

        void CollectFreeObjects(RenderPasses renderPass, List<Renderable> renderables)
        {
            foreach (Renderable r in _freeRenderables)
            {
                if ((r.RenderPasses & renderPass) != 0)
                {
                    renderables.Add(r);
                }
            }
        }

        Func<CullRenderable, bool> GetFilter(RenderPasses passes)
        {
            return _filters.GetOrAdd(passes, s_createFilterFunc);
        }

        static Func<CullRenderable, bool> CreateFilter(RenderPasses rp)
        {
            // This cannot be inlined into GetFilter -- a Roslyn bug causes copious allocations.
            // https://github.com/dotnet/roslyn/issues/22589
            return cr => (cr.RenderPasses & rp) == rp;
        }

        internal void DestroyAllDeviceObjects()
        {
            _cullableStage[0].Clear();
            _octree.GetAllContainedObjects(_cullableStage[0]);
            foreach (CullRenderable cr in _cullableStage[0])
            {
                cr.DestroyDeviceObjects();
            }
            foreach (Renderable r in _freeRenderables)
            {
                r.DestroyDeviceObjects();
            }

            _resourceUpdateCL.Dispose();
        }

        internal void CreateAllDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            _cullableStage[0].Clear();
            _octree.GetAllContainedObjects(_cullableStage[0]);
            foreach (CullRenderable cr in _cullableStage[0])
            {
                cr.CreateDeviceObjects(gd, cl, sc);
            }
            foreach (Renderable r in _freeRenderables)
            {
                r.CreateDeviceObjects(gd, cl, sc);
            }

            _resourceUpdateCL = gd.ResourceFactory.CreateCommandList();
            _resourceUpdateCL.Name = "Scene Resource Update Command List";
        }

        class RenderPassesComparer : IEqualityComparer<RenderPasses>
        {
            public bool Equals(RenderPasses x, RenderPasses y) => x == y;
            public int GetHashCode(RenderPasses obj) => ((byte)obj).GetHashCode();
        }
    }
}
