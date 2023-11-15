using System;

namespace HECSFramework.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FeatureAttribute : Attribute
    {
        public readonly string[] Features;

        public FeatureAttribute(params string[] features)
        {
            Features = features;
        }
    }
}
