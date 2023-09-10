using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public static class GlobalUpdateExtentions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HECSJobRun<T> RunJob<T>(this T job, World world) where T : struct, IHecsJob
        {
            return world.GlobalUpdateSystem.RunJob(job);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HECSJobRun<T> RunJob<T>(this T job) where T : struct, IHecsJob
        {
            return EntityManager.Default.GlobalUpdateSystem.RunJob(job);
        }
    }
}