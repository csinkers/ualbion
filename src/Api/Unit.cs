using System;

namespace UAlbion.Api;

public readonly struct Unit : IEquatable<Unit>
{
    // ReSharper disable once UnassignedReadonlyField (It's a struct, so it gets default initialised. Resharper's just being silly here)
    public static readonly Unit V;
    public override bool Equals(object obj) => obj is Unit;
    public override int GetHashCode() => 0;
    public static bool operator ==(Unit left, Unit right) => left.Equals(right);
    public static bool operator !=(Unit left, Unit right) => !(left == right);
    public bool Equals(Unit other) => true;
    public override string ToString() => "()";
}