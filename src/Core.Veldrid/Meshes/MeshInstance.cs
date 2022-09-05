using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using Veldrid.Utilities;

namespace UAlbion.Core.Veldrid.Meshes;

public class MeshInstance : Component, IMeshInstance, IRenderable
{
    readonly Mesh _mesh;
    readonly Vector3 _scale;
    BatchLease<MeshId, GpuMeshInstanceData> _lease;
    BoundingBox _boundingBox;
    Vector3 _position;

    public MeshInstance(Mesh mesh, Vector3 position, Vector3 scale)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _scale = scale; // Set before Position so the BoundingBox will be right
        Position = position;

        On<WorldCoordinateSelectEvent>(Select);

        // On<HoverEvent>(_ =>
        // {
        //     if ((GetVar(CoreVars.User.EngineFlags) & EngineFlags.HighlightSelection) == EngineFlags.HighlightSelection)
        //         Flags |= SpriteFlags.Highlight;
        // });
        // On<BlurEvent>(_ =>
        // {
        //     if ((GetVar(CoreVars.User.EngineFlags) & EngineFlags.HighlightSelection) == EngineFlags.HighlightSelection)
        //         Flags &= ~SpriteFlags.Highlight;
        // });
    }

    public MeshId Id => _mesh.Id;
    public string Name => _mesh.Id.ToString();
    public DrawLayer RenderOrder => _mesh.Id.RenderOrder;
    public Func<object> SelectionCallback { get; init; }
    public Vector3 Position
    {
        get => _position;
        set
        {
            if (_position == value)
                return;

            _position = value;
            if (_lease != null)
                _lease.Update(0, new GpuMeshInstanceData { Position = value });

            RebuildBox();
            if (IsSubscribed)
                Raise(new PositionedComponentMovedEvent(this));
        }
    }

    public void SetPosition(Vector3 position) => Position = position;
    public Vector3 Dimensions => _boundingBox.Max - _boundingBox.Min;

    protected override void Subscribed()
    {
        var manager = Resolve<IBatchManager<MeshId, GpuMeshInstanceData>>();
        _lease = manager.Borrow(_mesh.Id, 1, this);
        _lease.Update(0, new GpuMeshInstanceData { Position = Position, Scale = _scale });
        Raise(new AddPositionedComponentEvent(this));
    }

    protected override void Unsubscribed()
    {
        Raise(new RemovePositionedComponentEvent(this));
        _lease.Dispose();
        _lease = null;
    }

    void Select(WorldCoordinateSelectEvent e)
    {
        var hit = RayIntersect(e.Origin, e.Direction);
        if (!hit.HasValue)
            return;

        object selected = this;
        if (SelectionCallback != null)
            selected = SelectionCallback();

        if (selected != null)
            e.Selections.Add(new Selection(hit.Value.Item2, hit.Value.Item1, this));
    }

    void RebuildBox()
    {
        var min = _mesh.BoxMin * _scale + _position;
        var max = _mesh.BoxMax * _scale + _position;
        _boundingBox = new BoundingBox(min, max);
    }

    public (float, Vector3)? RayIntersect(Vector3 origin, Vector3 direction)
    {
        var ray = new Ray(origin, direction);
        if (!ray.Intersects(ref _boundingBox, out var t) || t < 0)
            return null;

        var intersectionPoint = origin + t * direction;
        return (t, intersectionPoint);
    }
}
