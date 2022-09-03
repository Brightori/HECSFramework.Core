using System;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public struct AsyncValueBuilder<T>
    {
        public static AsyncValueBuilder<T> Create() => new AsyncValueBuilder<T>(new Awaiter<T>());

        private readonly Awaiter<T> _awaiter;

        public AsyncValueBuilder(Awaiter<T> awaiter)
        {
            _awaiter = awaiter;
        }


        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        public void SetException(Exception exception)
        {
            _awaiter.ThrowException(exception);
        }

        public void SetResult(T result)
        {
            _awaiter.Complete(result);  
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public Awaiter<T> Task => _awaiter;
    }

    public struct AsyncBuilder
    {
        public static AsyncBuilder Create() => new AsyncBuilder(new Awaiter());

        private readonly Awaiter _awaiter;

        public AsyncBuilder(Awaiter awaiter)
        {
        _awaiter = awaiter;
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        public void SetException(Exception exception)
        {
            _awaiter.ThrowException(exception);
        }

        public void SetResult()
        {
            _awaiter.Complete();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public Awaiter Task => _awaiter;
    }
}