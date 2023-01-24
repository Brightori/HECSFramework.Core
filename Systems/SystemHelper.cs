using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public static class SystemHelper<T> where T : class, ISystem, new()
    {
        public static int TypeCode = IndexGenerator.GetIndexForType(typeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetSystemFromPool(World world)
        {
            return world.GetSystemFromPool<T>(TypeCode);
        }
    }

    public static class SystemHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReturnToPool(this ISystem system) 
        {
            system.Owner.World.ReturnSystemToPool(system.GetTypeHashCode, system);
        }
    }
}