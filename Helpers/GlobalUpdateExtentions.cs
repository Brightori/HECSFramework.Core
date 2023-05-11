using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HECSFramework.Core
{
    public static class GlobalUpdateExtentions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HECSJobRun<T> RunJob<T>(this T job, World world) where T : struct, IHecsJob
        {
            return world.GlobalUpdateSystem.RunJob(job);
        }
    }
}