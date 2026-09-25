using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    internal static partial class TypeContainersRegistry
    {
        public static readonly ITypeContainer[] All;

        //no initializer: parts compile in any order, and one that ran first would lose its registrations
        private static List<ITypeContainer> collected;

        //explicit on purpose: without it the type is beforefieldinit and the generated initializers may not run before All is read
        static TypeContainersRegistry()
        {
            All = collected != null ? collected.ToArray() : Array.Empty<ITypeContainer>();
            collected = null;
        }

        private static bool Add(ITypeContainer container)
        {
            (collected ??= new List<ITypeContainer>(512)).Add(container);
            return true;
        }
    }
}
