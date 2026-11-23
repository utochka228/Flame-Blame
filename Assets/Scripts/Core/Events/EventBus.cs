using System;
using System.Collections.Generic;

namespace FlameBlame.Core.Events
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Subscribers = new Dictionary<Type, Delegate>();

        public static void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);
            Subscribers.TryGetValue(eventType, out var existing);
            Subscribers[eventType] = (Action<TEvent>)existing + handler;
        }

        public static void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);
            if (!Subscribers.TryGetValue(eventType, out var existing)) return;

            var updated = (Action<TEvent>)existing - handler;
            if (updated == null) Subscribers.Remove(eventType);
            else Subscribers[eventType] = updated;
        }

        public static void Publish<TEvent>(TEvent eventData)
        {
            if (Subscribers.TryGetValue(typeof(TEvent), out var existing))
            {
                ((Action<TEvent>)existing)?.Invoke(eventData);
            }
        }
    }
}
