using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HECSFramework.Core
{
    public static class GlobalUpdateExtentions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async ValueTask<T> RunJob<T>(this T job, World world) where T : struct, IHecsJob
        {
            return await world.GlobalUpdateSystem.RunJob(job);
        }
    }
}