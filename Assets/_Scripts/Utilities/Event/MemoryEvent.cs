using System;
using System.Collections.Generic;

namespace Utilities.Event
{
    /// <summary>
    /// Event that keeps previous events in memory to send to new subscribers.
    /// </summary>
    public struct MemoryEvent<T>
    {
        private event Action<T> OnEvent;
        private List<T> memory;

        public MemoryEvent(MemoryEvent<T> other)
        {
            OnEvent = default;
            memory = other.memory == null ? new List<T>() : new List<T>(other.memory);
            
            if (other.OnEvent != null)
            {
                foreach (var eventDelegate in other.OnEvent.GetInvocationList())
                {
                    OnEvent += (Action<T>) eventDelegate;
                }
            }
        }

        /// <summary>
        /// Invoke the event, keeping the parameter in memory for future subscribers
        /// </summary>
        public void InvokeWithMemory(T parameter)
        {
            memory ??= new List<T>();
            
            memory.Add(parameter);
            OnEvent?.Invoke(parameter);
        }

        /// <summary>
        /// Invoke the event without keeping the parameter in memory
        /// The future subscribers will never be notified of this call.
        /// </summary>
        public void InvokeWithoutMemory(T parameter)
        {
            OnEvent?.Invoke(parameter);
        }

        /// <summary>
        /// Clear the event's memory, preventing future subscriber from being notified of past calls.
        /// </summary>
        public void ClearMemory()
        {
            memory.Clear();
        }

        /// <summary>
        /// Remove an element from memory that will not be sent to future subscribers
        /// </summary>
        public void RemoveFromMemory(T elementToRemove)
        {
            if (memory == null)
                return;

            memory.Remove(elementToRemove);
        }

        private void AddAction(Action<T> action)
        {
            foreach (var element in memory)
            {
                action?.Invoke(element);   
            }
            
            OnEvent += action;
        }
        
        private void RemoveAction(Action<T> action)
        {
            OnEvent -= action;
        }

        public static MemoryEvent<T> operator +(MemoryEvent<T> memoryEvent, Action<T> action)
        {
            var newMemoryEvent = new MemoryEvent<T>(memoryEvent);
            newMemoryEvent.AddAction(action);
            
            return newMemoryEvent;
        }
        
        public static MemoryEvent<T> operator -(MemoryEvent<T> memoryEvent, Action<T> action)
        {
            var newMemoryEvent = new MemoryEvent<T>(memoryEvent);
            newMemoryEvent.RemoveAction(action);
            
            return newMemoryEvent;
        }
    }
    
    /// <summary>
    /// Event that keeps previous events in memory to send to new subscribers.
    /// </summary>
    public struct MemoryEvent
    {
        private event Action OnEvent;
        private int memory;

        public MemoryEvent(MemoryEvent other)
        {
            OnEvent = default;
            memory = other.memory;
            
            if (other.OnEvent != null)
            {
                foreach (var eventDelegate in other.OnEvent.GetInvocationList())
                {
                    OnEvent += (Action) eventDelegate;
                }
            }
        }

        /// <summary>
        /// Invoke the event, keeping the call in memory for future subscribers
        /// </summary>
        public void InvokeWithMemory()
        {
            memory++;
            OnEvent?.Invoke();
        }

        /// <summary>
        /// Invoke the event without keeping the call in memory
        /// The future subscribers will never be notified of this call.
        /// </summary>
        public void InvokeWithoutMemory()
        {
            OnEvent?.Invoke();
        }

        /// <summary>
        /// Clear the event's memory, preventing future subscriber from being notified of past calls.
        /// </summary>
        public void ClearMemory()
        {
            memory = 0;
        }

        /// <summary>
        /// Remove a call from memory that will not be sent to future subscribers
        /// </summary>
        public void RemoveFromMemory()
        {
            memory = Math.Max(0, memory - 1);
        }

        private void AddAction(Action action)
        {
            for (var i = 0; i < memory; ++i)
            {
                action?.Invoke();
            }
            
            OnEvent += action;
        }
        
        private void RemoveAction(Action action)
        {
            OnEvent -= action;
        }

        public static MemoryEvent operator +(MemoryEvent memoryEvent, Action action)
        {
            var newMemoryEvent = new MemoryEvent(memoryEvent);
            newMemoryEvent.AddAction(action);
            
            return newMemoryEvent;
        }
        
        public static MemoryEvent operator -(MemoryEvent memoryEvent, Action action)
        {
            var newMemoryEvent = new MemoryEvent(memoryEvent);
            newMemoryEvent.RemoveAction(action);
            
            return newMemoryEvent;
        }
    }
}