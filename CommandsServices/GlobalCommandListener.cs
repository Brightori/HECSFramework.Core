using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public sealed partial class GlobalCommandListener<T> : IDisposable where T : struct, IGlobalCommand
    {
        public static HECSList<GlobalCommandListener<T>> ListenersToWorld = new HECSList<GlobalCommandListener<T>>(4);
        public HECSList<IReactGlobalCommand<T>> listeners = new HECSList<IReactGlobalCommand<T>>(16);

        private Queue<IReactGlobalCommand<T>> listenersToRemove = new Queue<IReactGlobalCommand<T>>(8);

        private bool isDirty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddListener(int index, IReactGlobalCommand<T> react)
        {
            if (index < ListenersToWorld.Count)
            {
                if (ListenersToWorld.Data[index] == null)
                    ListenersToWorld.Data[index] = new GlobalCommandListener<T>();
            }
            else
                ListenersToWorld.AddToIndex(new GlobalCommandListener<T>(), index);

            ListenersToWorld.Data[index].AddListener(react);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveListener(int index, IReactGlobalCommand<T> react)
        {
            if (index < ListenersToWorld.Count)
            {
                if (ListenersToWorld.Data[index] == null)
                    ListenersToWorld.Data[index] = new GlobalCommandListener<T>();
            }
            else
                ListenersToWorld.AddToIndex(new GlobalCommandListener<T>(), index);

            ListenersToWorld.Data[index].RemoveListener(react);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveListener(int index, ISystem react)
        {
            if (index < ListenersToWorld.Count)
            {
                if (ListenersToWorld.Data[index] == null)
                    ListenersToWorld.Data[index] = new GlobalCommandListener<T>();
            }
            else
                ListenersToWorld.AddToIndex(new GlobalCommandListener<T>(), index);

            ListenersToWorld.Data[index].RemoveListener(react);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invoke(int WorldIndex, T command)
        {
            if (WorldIndex < ListenersToWorld.Count)
            {
                if (ListenersToWorld.Data[WorldIndex] != null)
                {
                    ListenersToWorld.Data[WorldIndex].Invoke(command);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddListener(IReactGlobalCommand<T> react)
        {
            listeners.Add(react);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(T data)
        {
            ProcessRemove();
            var count = listeners.Count;

            for (int i = 0; i < count; i++)
            {
                var listener = listeners.Data[i];

                if (listener == null || !listener.Owner.IsAlive())
                {
                    listenersToRemove.Enqueue(listener);
                    isDirty = true;
                    continue;
                }

                if (listener.Owner.IsPaused)
                    continue;

                listener.CommandGlobalReact(data);
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
                    listeners.RemoveSwap(remove);
                }

                isDirty = false;
            }
        }

        public void RemoveListener(IHaveOwner listener)
        {
            foreach (var react in listeners)
            {
                if (react.Owner == listener.Owner)
                    listenersToRemove.Enqueue(listener as IReactGlobalCommand<T>);
            }

            isDirty = true;
            ProcessRemove();
        }

        public void RemoveListener(ISystem listener)
        {
            foreach (var react in listeners)
            {
                if (react.Owner == listener.Owner)
                    listenersToRemove.Enqueue(listener as IReactGlobalCommand<T>);
            }

            isDirty = true;
            ProcessRemove();
        }

        public void RemoveListener(IReactGlobalCommand<T> listener)
        {
            foreach (var react in listeners)
            {
                if (react == listener)
                    listenersToRemove.Enqueue(listener);
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