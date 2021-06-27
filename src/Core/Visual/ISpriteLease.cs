using System;
using System.Numerics;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual
{
    public interface ISpriteLease : IDisposable
    {
        SpriteKey Key { get; }
        int Length { get; }
        void Update(int index, Vector3 position, Vector2 size, Region region, SpriteFlags flags);
        void Update(int index, Vector3 position, Vector2 size, int regionIndex, SpriteFlags flags);
        void UpdateFlags(int index, SpriteFlags flags, SpriteFlags? mask = null);
        void OffsetAll(Vector3 offset);
        IWeakSpriteReference MakeWeakReference(int index);

        /// <summary>
        /// A delegate type for sprite instance data mutation functions. Accepts a span of the
        /// lease's instances as well as an arbitrary context object.
        /// </summary>
        /// <typeparam name="T">The type of the context object</typeparam>
        /// <param name="instances">A span pointing at the lease's instance data</param>
        /// <param name="context">The context for the mutator function</param>
        delegate void LeaseAccessDelegate<in T>(Span<SpriteInstanceData> instances, T context);

        /// <summary>
        /// Invokes the mutator function with a span of the lease's instance
        /// data to allow modification. An arbitrary context object can also be
        /// passed in to avoid allocation of a closure.
        /// </summary>
        /// <typeparam name="T">The type of the context object</typeparam>
        /// <param name="mutatorFunc">The function used to modify the instance data</param>
        /// <param name="context">The context for the mutator function</param>
        void Access<T>(LeaseAccessDelegate<T> mutatorFunc, T context);

        /// <summary>
        /// Locks the sprite lease's underlying memory to ensure that it
        /// won't be moved until Unlock is called. It is the caller's
        /// responsibility to always call Unlock before the Span goes out
        /// of scope (and pass through lockWasTaken as returned by Lock).
        /// Care should be taken that this happens even if an exception is
        /// thrown, e.g. by using a try...finally.
        /// </summary>
        /// <param name="lockWasTaken"></param>
        /// <returns>A span containing the lease's instance data</returns>
        Span<SpriteInstanceData> Lock(ref bool lockWasTaken);

        /// <summary>
        /// Releases the lock on the lease's underlying memory.
        /// </summary>
        /// <param name="lockWasTaken">This should be the value returned as a ref parameter from Lock. It is important for re-entrant calls.</param>
        void Unlock(bool lockWasTaken);
    }
}