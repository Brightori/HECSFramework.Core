using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace HECSFramework.Core
{
    [AsyncMethodBuilder(typeof(AsyncBuilder))]
    public class Awaiter : IAwaiter, INotifyCompletion
    {
        private Action _continuation;
        private bool _isCompleted = false;
        private Exception _exception;

        public virtual bool IsCompleted => _isCompleted;
        public bool GetResult() => _isCompleted;
        public Awaiter GetAwaiter() => this;


        public Awaiter()
        {
            JobsSystem.RegisterAwaiter(this);
        }

        public void OnCompleted(Action continuation)
        {
            _continuation += continuation;
        }


        public void ThrowException(Exception e)
        {
            _exception = e;
            _isCompleted = true;
        }

        public void Complete()
        {
            _isCompleted = true;
        }

        public bool TryFinalize()
        {
            if (IsCompleted)
            {
                _continuation?.Invoke();
                _continuation = null;
                return true;
            }
            return false;
        }
    }

    [AsyncMethodBuilder(typeof(AsyncValueBuilder<>))]
    public class Awaiter<T> : IAwaiter, INotifyCompletion
    {

        private Action _continuation;
        private bool _isCompleted = false;
        private T _result;
        private Exception _exception;

        public bool IsCompleted => _isCompleted;

        public Awaiter<T> GetAwaiter() => this;

        public Awaiter()
        {
            JobsSystem.RegisterAwaiter(this);
        }

        public T GetResult()
        {
            if (!_isCompleted) throw new Exception();
            if (_exception != null) ExceptionDispatchInfo.Throw(_exception); ;

            return _result;
        }

        public void OnCompleted(Action continuation) 
        {
            _continuation += continuation;
        }
       

        public void ThrowException(Exception e)
        {
            _exception = e;
            _isCompleted = true;
        }

        public void Complete(T result)
        {
            _result = result;
            _isCompleted = true;
        }

        public bool TryFinalize()
        {
            if (IsCompleted)
            {
                _continuation?.Invoke();
                _continuation = null;
                return true;
            }
            return false;
        }
    }
}
