using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{

    [Documentation(Doc.GameLogic, Doc.HECS, "Это старая реализация, где мы глобально подписываемся на все компоненты подряд")]
    public sealed partial class ComponentsService : IDisposable
    {
        private World world;
        private Dictionary<Type, HashSet<ComponentProvider>> typeToProviders;

        public ComponentsService(World world)
        {
            this.world = world;
        }

        public void AddListener<T>(IReactComponentGlobal<T> listener, bool add) where T: IComponent
        {
            ComponentProvider<T>.ComponentsToWorld.Data[world.Index].AddGlobalComponentListener(listener, add);
        }

        public void AddGenericListener<T>(IReactGenericGlobalComponent<T> listener, bool add)
        {
            var key = typeof(T);
            if (typeToProviders.TryGetValue(key, out var listeners))
            {
                foreach (var provider in listeners)
                    provider.AddGlobalUniversalListener(listener, add);
                
                return;
            }

            typeToProviders.Add(key, new HashSet<ComponentProvider>(8));

            foreach (var c in world.ComponentProviders)
            {
                if (c.IsNeededType<T>())
                    typeToProviders[key].Add(c);
            }
        }

        public void Dispose()
        {
            world = null;
        }

        internal void AddLocalListener<T>(int entity, IReactComponentLocal<T> action, bool add) where T : IComponent
        {
            ComponentProvider<T>.ComponentsToWorld.Data[world.Index].AddGlobalComponentListener
        }
    }
}