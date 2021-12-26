using System;

namespace UAlbion.Core
{
    static class DummyContinuation
    {
        public static Action Instance { get; } = () => { };
    }

    static class DummyContinuation<T>
    {
        public static Action<T> Instance { get; } = _ => { };
    }
}