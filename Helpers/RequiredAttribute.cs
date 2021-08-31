using System;

namespace HECSFramework.Core
{
    /// <summary>
    /// это для контейнеров акторов и абилок, тут мы гарантируем что эти компоненты будут в контейнере
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredAttribute : Attribute
    {
    }

    /// <summary>
    /// атрибут для случаев если нам не нужны закэшированные поля в классе, но мы к этим компонентам обращаемся рантайм
    /// и нужно гарантировать что у контейнера есть необходимые компоненты/системы
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequiredAtContainerAttribute : Attribute
    {
        public readonly Type[] neededTypes;

        public RequiredAtContainerAttribute(params Type[] neededTypes)
        {
            this.neededTypes = neededTypes;
        }
    }
}
