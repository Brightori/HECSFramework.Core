using System;

namespace HECSFramework.Core
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class HideInInspectorCrossPlatform : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ReadOnlyCrossPlatform : Attribute
    {
    }
}