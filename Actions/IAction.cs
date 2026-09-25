using System.Threading;
#if UNITY_2017_1_OR_NEWER
using Cysharp.Threading.Tasks;
#endif

namespace HECSFramework.Core
{
    public interface IAction
    {
        public void Action(Entity owner, Entity target = null);
    }

#if UNITY_2017_1_OR_NEWER
    public interface IAsyncAction
    {
        /// <summary>
        /// here we provide target and owner
        /// </summary>
        /// <param name="to">target</param>
        /// <param name="from">owner</param>
        /// <returns></returns>
        public UniTask ActionAsync(Entity to, Entity from = null, CancellationToken cancellationToken = default);
    }
#endif
}
