using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public static class SystemHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReturnToPool(this ISystem system) 
        {
            system.Owner.World.ReturnSystemToPool(system.GetTypeHashCode, system);
        }
    }
}