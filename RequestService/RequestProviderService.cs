using System;
using System.Runtime.CompilerServices;
using HECSFramework.Core;

namespace HECSFramework.Core
{
    public abstract class RequestProvider : IRequestProvider
    {
        public abstract void Dispose();
    }

    public sealed partial class RequestProviderService<T, U> : RequestProvider, IDisposable where U : struct, ICommand
    {
        private static HECSList<RequestProviderService<T, U>> ProcessorsToWorld = new(4);

        private IRequestProvider<T, U> requestProcessor;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRequestProcessor(int index, IRequestProvider<T, U> react)
        {
            if (index < ProcessorsToWorld.Count)
                ProcessorsToWorld.Data[index].AddRequestProcessor(react);
            else
            {
                ProcessorsToWorld.AddToIndex(new RequestProviderService<T, U>(), index);
                ProcessorsToWorld.Data[index].AddRequestProcessor(react);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveRequestProcessor(int index)
        {
            if (index < ProcessorsToWorld.Count)
                ProcessorsToWorld.Data[index].requestProcessor = null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Request(int worldIndex, U data)
        {
            return ProcessorsToWorld.Data[worldIndex].Request(data);
        }

        public void AddRequestProcessor(IRequestProvider<T, U> react)
        {
            if (requestProcessor != null)
                throw new Exception($"processor alrdy setted {typeof(T).Name} {typeof(U).Name}");

            requestProcessor = react;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Request(U data)
        {
            return requestProcessor.Request(data);
        }

        public override void Dispose()
        {
            ProcessorsToWorld.Clear();
            requestProcessor = null;
        }
    }

    public sealed partial class RequestProviderService<T> : RequestProvider, IDisposable 
    {
        private static HECSList<RequestProviderService<T>> ProcessorsToWorld = new(4);

        private IRequestProvider<T> requestProcessor;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRequestProcessor(int index, IRequestProvider<T> react)
        {
            if (index < ProcessorsToWorld.Count)
                ProcessorsToWorld.Data[index].AddRequestProcessor(react);
            else
            {
                ProcessorsToWorld.AddToIndex(new RequestProviderService<T>(), index);
                ProcessorsToWorld.Data[index].AddRequestProcessor(react);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveRequestProcessor(int index)
        {
            if (index < ProcessorsToWorld.Count)
                ProcessorsToWorld.Data[index].requestProcessor = null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Request(int worldIndex)
        {
            return ProcessorsToWorld.Data[worldIndex].Request();
        }

        public void AddRequestProcessor(IRequestProvider<T> react)
        {
            if (requestProcessor != null)
                throw new Exception($"processor alrdy setted {typeof(T).Name}");

            requestProcessor = react;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Request()
        {
            return requestProcessor.Request();
        }

        public override void Dispose()
        {
            ProcessorsToWorld.Clear();
            requestProcessor = null;
        }
    }
}

public interface IRequestProvider : IDisposable
{
}

public interface IRequestProvider<T> : IRequestProvider, ISystem 
{
    T Request();
}

public interface IRequestProvider<T, U> : IRequestProvider, ISystem where U : struct, ICommand
{
    T Request(U command);
}