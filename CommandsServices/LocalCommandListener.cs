using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public sealed partial class LocalCommandListener<T> : IDisposable where T : struct, ICommand
    {
        public static HECSList<LocalCommandListener<T>> ListenersToWorld = new HECSList<LocalCommandListener<T>>(4);

        public Dictionary<int, HECSList<IReactCommand<T>>> listeners = new Dictionary<int, HECSList<IReactCommand<T>>>(16);
        private Queue<(int entityIndex, IReactCommand<T>)> listenersToRemove = new Queue<(int entityIndex, IReactCommand<T>)>(8);

        private bool isDirty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddListener(int index, IReactCommand<T> react)
        {
            if (index < ListenersToWorld.Count)
            {
                if (ListenersToWorld.Data[index] == null)
                    ListenersToWorld.Data[index] = new LocalCommandListener<T>();
            }
            else
                ListenersToWorld.AddToIndex(new LocalCommandListener<T>(), index);

            ListenersToWorld.Data[index].AddListener(react);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveListener(int index, ISystem react)
        {
            if (index < ListenersToWorld.Count)
            {
                if (ListenersToWorld.Data[index] == null)
                    ListenersToWorld.Data[index] = new LocalCommandListener<T>();
            }
            else
                ListenersToWorld.AddToIndex(new LocalCommandListener<T>(), index);

            ListenersToWorld.Data[index].RemoveListener(react);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invoke(int WorldIndex, int entityIndex, T command)
        {
            if (WorldIndex < ListenersToWorld.Count)
            {
                if (ListenersToWorld.Data[WorldIndex] != null)
                {
                    ListenersToWorld.Data[WorldIndex].Invoke(entityIndex, command);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddListener(IReactCommand<T> react)
        {
            var entityIndex = react.Owner.Index;

            if (listeners.ContainsKey(entityIndex))
                listeners[entityIndex].Add(react);
            else
                listeners.Add(entityIndex, new HECSList<IReactCommand<T>> { react });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(int entity, T data)
        {
            ProcessRemove();

            if (this.listeners.TryGetValue(entity, out var listeners))
            {
                var count = listeners.Count;

                for (int i = 0; i < count; i++)
                {
                    var listener = listeners.Data[i];

                    if (listener == null || !listener.Owner.IsAlive())
                        continue;

                    listener.CommandReact(data);
                }
            }

            ProcessRemove();
        }

        private void ProcessRemove()
        {
            if (isDirty)
            {
                while (listenersToRemove.Count > 0)
                {
                    var remove = listenersToRemove.Dequeue();
                    listeners[remove.entityIndex].RemoveSwap(remove.Item2);
                }

                isDirty = false;
            }
        }

        public void RemoveListener(ISystem listener)
        {
            if (this.listeners.TryGetValue(listener.Owner.Index, out var listeners))
            {
                foreach (var react in listeners)
                {
                    if (react.Owner.GUID == listener.Owner.GUID)
                        listenersToRemove.Enqueue((listener.Owner.Index, listener as IReactCommand<T>));
                }
            }
            isDirty = true;
            ProcessRemove();
        }

        public void RemoveReactListener(IReactCommand<T> listener)
        {
            if (this.listeners.TryGetValue(listener.Owner.Index, out var listeners))
            {
                foreach (var react in listeners)
                {
                    if (react.Owner.GUID == listener.Owner.GUID)
                        listenersToRemove.Enqueue((listener.Owner.Index, listener));
                }
            }

            isDirty = true;
            ProcessRemove();
        }

        public void Dispose()
        {
            listeners.Clear();
            listenersToRemove.Clear();
        }
    }
}