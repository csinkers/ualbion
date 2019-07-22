using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Core
{
    public class EventExchange
    {
        readonly IDictionary<Type, IList<IComponent>> _subscriptions = new Dictionary<Type, IList<IComponent>>();
        readonly IDictionary<IComponent, IList<Type>> _subscribers = new Dictionary<IComponent, IList<Type>>();
        readonly object _syncRoot = new object();

        public void Raise(IEvent e, object sender)
        {
            HashSet<IComponent> subscribers = new HashSet<IComponent>();
            var interfaces = e.GetType().GetInterfaces();

            lock (_syncRoot)
            {
                if (_subscriptions.TryGetValue(e.GetType(), out var tempSubscribers))
                    foreach (var subscriber in tempSubscribers)
                        subscribers.Add(subscriber);

                foreach(var @interface in interfaces)
                    if (_subscriptions.TryGetValue(@interface, out var interfaceSubscribers))
                        foreach (var subscriber in interfaceSubscribers)
                            subscribers.Add(subscriber);
            }

            if (subscribers != null)
                foreach(var subscriber in subscribers)
                    if(subscriber != sender)
                        subscriber.Receive(e);
        }

        public void Subscribe<T>(IComponent subscriber)
        {
            lock (_syncRoot)
            {
                if (_subscribers.TryGetValue(subscriber, out var subscribedTypes))
                {
                    if (subscribedTypes.Contains(typeof(T)))
                        return;
                }
                else _subscribers[subscriber] = new List<Type>();

                if (!_subscriptions.ContainsKey(typeof(T)))
                    _subscriptions.Add(typeof(T), new List<IComponent>());

                _subscriptions[typeof(T)].Add(subscriber);
                _subscribers[subscriber].Add(typeof(T));
            }
        }

        public void Unsubscribe(IComponent subscriber)
        {
            lock (_syncRoot)
            {
                if (!_subscribers.TryGetValue(subscriber, out var subscribedTypes))
                    return;

                foreach (var type in subscribedTypes.ToList())
                    _subscriptions[type].Remove(subscriber);

                _subscribers.Remove(subscriber);
            }
        }

        public void Unsubscribe<T>(IComponent subscriber)
        {
            lock (_syncRoot)
            {
                if (!_subscribers.TryGetValue(subscriber, out var subscribedTypes))
                    return;

                subscribedTypes.Remove(typeof(T));
                _subscriptions[typeof(T)].Remove(subscriber);
            }
        }
    }

    /*

    GameSystem: Update(float), bool Enabled, OnNewSceneLoaded()
        GraphicsSystem
        InputSystem
        SceneLoaderSystem
        AudioSystem
        AssetSystem
        BehaviorUpdateSystem
            IUpdateable[] + New & Removed (blocking)
            Behavior[] newStarts
            Update: Calls Update on all updateables and then flushes pending change lists

        PhysicsSystem
        ConsoleCommandSystem
        SynchronizationHelperSystem (allows invoking actions on main thread)
        GameObjectQuerySystem

    GameObject - owns a collection of components, has a transform and parent/children

    IUpdateable: Update(float)

    Component: GameObject owner, Transform, Attached/Removed, OnEnabled/Disabled, bool Enabled
        Behaviour: Start(Registry), PostEnabled/Disabled, PostAttached/Removed



    public interface ISubsystem { }

    public static class Registry
    {
        static readonly IDictionary<Type, ISubsystem> Subsystems = new Dictionary<Type, ISubsystem>();
        static readonly object SyncRoot = new object();

        public static T Resolve<T>()
        {
            lock (SyncRoot)
            {
                return (T)Subsystems[typeof(T)];
            }
        }

        public static void Register(ISubsystem subsystem)
        {
            lock (SyncRoot)
            {
                var type = subsystem.GetType();
                if(Subsystems.ContainsKey(type))
                    throw new InvalidOperationException($"Component {type} already registered");

                Subsystems[type] = subsystem;
            }
        }
    }
    */
}
