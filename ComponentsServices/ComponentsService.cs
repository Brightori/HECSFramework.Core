using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{

    [Documentation(Doc.GameLogic, Doc.HECS, "Это старая реализация, где мы глобально подписываемся на все компоненты подряд")]
    public sealed partial class ComponentsService : IDisposable
    {
        private World world;
        private Dictionary<Type, HashSet<ComponentProvider>> typeToProviders = new Dictionary<Type, HashSet<ComponentProvider>>(256);

        public ComponentsService(World world)
        {
            this.world = world;
        }
       
        public void Dispose()
        {
            world = null;
            typeToProviders.Clear();
        }

        internal void AddLocalListener<T>(int entity, IReactComponentLocal<T> action, bool add) where T : IComponent
        {
            ComponentProvider<T>.ComponentsToWorld.Data[world.Index].AddLocalComponentListener(entity, action, add);
        }

        public void AddListener<T>(IReactComponentGlobal<T> listener, bool add) where T : IComponent
        {
            ComponentProvider<T>.ComponentsToWorld.Data[world.Index].AddGlobalComponentListener(listener, add);
        }

        public void AddGenericListener<T>(IReactGenericGlobalComponent<T> listener, bool add)
        {
            var key = typeof(T);
            if (typeToProviders.TryGetValue(key, out var listeners))
            {
                foreach (var provider in listeners)
                    provider.AddGlobalGenericListener(listener, add);

                return;
            }

            typeToProviders.Add(key, new HashSet<ComponentProvider>(8));

            foreach (var c in world.ComponentProviders)
            {
                if (c.IsNeededType<T>())
                    typeToProviders[key].Add(c);
            }
            foreach (var provider in typeToProviders[key])
                provider.AddGlobalGenericListener(listener, add);
        }

        internal void AddLocalGenericListener<T>(int index, IReactGenericLocalComponent<T> reactComponent, bool added)
        {
            var key = typeof(T);
            if (typeToProviders.TryGetValue(key, out var listeners))
            {
                foreach (var provider in listeners)
                    provider.AddLocalGenericListener(index, reactComponent, added);

                return;
            }

            typeToProviders.Add(key, new HashSet<ComponentProvider>(8));

            foreach (var c in world.ComponentProviders)
            {
                if (c.IsNeededType<T>())
                    typeToProviders[key].Add(c);
            }

            foreach (var provider in typeToProviders[key])
                provider.AddLocalGenericListener(index, reactComponent, added);
        }
    }
}