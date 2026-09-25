using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core.Generator
{
    public partial class CodeGenerator
    {
#pragma warning disable
        private readonly string DefaultNameSpace = "HECSFramework.Core";
#pragma warning enable

        private List<Type> componentTypes;
        private List<Type> systems;
        public static IEnumerable<Type> Assembly;

        public void GatherAssembly()
        {
            var componentType = typeof(IComponent);
            var systemsTypes = typeof(ISystem);

            Assembly = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes());
            componentTypes = Assembly.Where(p => componentType.IsAssignableFrom(p) && !p.IsGenericType && !p.IsAbstract && !p.IsInterface).ToList();
            systems = Assembly.Where(p => systemsTypes.IsAssignableFrom(p) && p.IsClass && !p.IsInterface && !p.IsAbstract).ToList();
        }
    }
}