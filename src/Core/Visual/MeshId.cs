using System;
using UAlbion.Api;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;

public readonly struct MeshId : IBatchKey, IEquatable<MeshId>
{
    public MeshId(IAssetId id) => Id = id;
    public IAssetId Id { get; }
    public DrawLayer RenderOrder => DrawLayer.Billboards;
    public bool Equals(MeshId other) => Id.Equals(other.Id);
    public override bool Equals(object obj) => obj is MeshId other && Equals(other);
    public static bool operator ==(MeshId x, MeshId y) => x.Equals(y);
    public static bool operator !=(MeshId x, MeshId y) => !(x == y);
    public override string ToString() => Id.ToString();
    public override int GetHashCode() => Id.GetHashCode();
}