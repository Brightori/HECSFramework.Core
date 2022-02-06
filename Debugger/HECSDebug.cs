using System;
using System.Diagnostics;

namespace HECSFramework.Core
{
    public static class HECSDebug
    {
        public static IDebugDispatcher Dispatcher { get; private set; } = new NullDebug();

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
        public static void LogDebug(object info, object context = null)
        {
            Dispatcher.LogDebug(info.ToString(), context);
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

//заглушка 
public class NullDebug : IDebugDispatcher
{
    public void Log(string info)
    {
        Console.WriteLine(info);
    }

    public void LogDebug(string info, object context)
    {
        Console.WriteLine(info);
    }

    public void LogError(string info)
    {
        Console.WriteLine(info);
    }

    public void LogWarning(string info)
    {
        Console.WriteLine(info);
    }
}

public interface IDebugDispatcher
{
    void LogDebug(string info, object context);
    void Log(string info);
    void LogWarning(string info);
    void LogError(string info);
}