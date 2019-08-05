using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Core.Events
{
    public class EventExchange
    {
        readonly IDictionary<Type, IList<IComponent>> _subscriptions = new Dictionary<Type, IList<IComponent>>();
        readonly IDictionary<IComponent, IList<Type>> _subscribers = new Dictionary<IComponent, IList<Type>>();
        readonly IDictionary<Type, RegisteredComponent> _registrations = new Dictionary<Type, RegisteredComponent>();
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
                    subscriber.Receive(e, sender);
        }

        public void Subscribe<T>(IComponent subscriber) { Subscribe(typeof(T), subscriber); }
        public void Subscribe(Type eventType, IComponent subscriber)
        {
            lock (_syncRoot)
            {
                if (_subscribers.TryGetValue(subscriber, out var subscribedTypes))
                {
                    if (subscribedTypes.Contains(eventType))
                        return;
                }
                else _subscribers[subscriber] = new List<Type>();

                if (!_subscriptions.ContainsKey(eventType))
                    _subscriptions.Add(eventType, new List<IComponent>());

                _subscriptions[eventType].Add(subscriber);
                _subscribers[subscriber].Add(eventType);
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

        public void Register(RegisteredComponent rc) { _registrations.Add(rc.GetType(), rc); }

        public T Resolve<T>() where T : RegisteredComponent
        {
            if (!_registrations.ContainsKey(typeof(T)))
                return null;
            return (T) _registrations[typeof(T)];
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
