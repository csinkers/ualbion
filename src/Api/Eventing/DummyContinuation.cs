using System;

namespace UAlbion.Api.Eventing;

static class DummyContinuation
{
    public static Action Instance { get; } = () => { };
}

static class DummyContinuation<T>
{
    public static Action<T> Instance { get; } = _ => { };
}