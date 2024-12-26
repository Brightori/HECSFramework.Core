using System.Threading;
using Cysharp.Threading.Tasks;

namespace HECSFramework.Core
{
    public interface IAction
    {
        public void Action(Entity entity);
    }

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
}