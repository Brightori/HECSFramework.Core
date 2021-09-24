using System;
using System.Diagnostics;

namespace HECSFramework.Core
{
    public static class HECSDebug
    {
        public static IDebugDispatcher Dispatcher { get; private set; }

        public static void Init(IDebugDispatcher debugDispatcher)
        {
            Dispatcher = debugDispatcher;
        }

        [Conditional("DEBUG")]
        public static void AssertNotNull(object check, string message = "")
        {
            if (check == null)
                throw new AssertionException(message);
        }

        [Conditional("DEBUG")]
        public static void AssertIsTrue(bool predicate, string message = "")
        {
            if (!predicate)
                throw new AssertionException(message);
        }

        [Conditional("DEBUG")]
        public static void LogDebug(object info)
        {
            Dispatcher.LogDebug(info.ToString());
        }
        
        public static void Log(object info)
        {
            Dispatcher.Log(info.ToString());
        }
        
        public static void LogWarning(object info)
        {
            Dispatcher.LogWarning(info.ToString());
        }
        
        public static void LogError(object info)
        {
            Dispatcher.LogError(info.ToString());
        }
    }
}

public interface IDebugDispatcher
{
    void LogDebug(string info);
    void Log(string info);
    void LogWarning(string info);
    void LogError(string info);
}