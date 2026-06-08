using System;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.HECS, "Commands", Doc.Proxy, "this system can add u ability to recieve local commands from other entity, u should inherit this system and use needed command")]
    public abstract class ProxyListenerSystem<T> : BaseSystem, IReactCommand<T> where T : struct, ICommand
    {
        private IProxyListener<T> proxyListener;
        private AliveEntity listener;

        public void CommandReact(T command)
        {
            if (listener.IsAlive)
            {
                proxyListener.OnProxyCommandReact(command);
            }
        }

        /// <summary>
        /// u should make empty constructor in child too, for blueprint
        /// </summary>
        /// <param name="proxyListener"></param>
        public ProxyListenerSystem(IProxyListener<T> proxyListener)
        {
            this.proxyListener = proxyListener;
            listener = proxyListener.Owner;
        }

        public override void InitSystem()
        {
        }
    }

    public interface IProxyListener<T> : IHaveOwner where T : struct, ICommand
    {
        public void OnProxyCommandReact(T command);
    }
}