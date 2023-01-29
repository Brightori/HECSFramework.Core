using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    internal sealed partial class ComponentProvider<T> : ComponentProvider, IPriorityUpdatable, IDisposable where T : IComponent
    {
        public static HECSList<ComponentProvider<T>> ComponentsToWorld = new HECSList<ComponentProvider<T>>(16);
        public T[] Components = new T[World.StartEntitiesCount];
        public World World;
        public static int TypeIndex = IndexGenerator.GetIndexForType(typeof(T));
        private HashSet<IReactComponentGlobal<T>> reactComponentGlobals = new HashSet<IReactComponentGlobal<T>>(8);
        private Dictionary<Type, UniversalReactGlobal> universalReactGlobals = new Dictionary<Type, UniversalReactGlobal>(8);
        private Dictionary<int, HashSet<IReactComponentLocal<T>>> localListeners = new Dictionary<int, HashSet<IReactComponentLocal<T>>>(4);

        private Queue<T> addedComponent = new Queue<T>(4);
        private Queue<(int,T, bool)> addLocalComponent = new Queue<(int, T, bool)>(4);
        private bool IsDirty;

        private T check;

        static ComponentProvider()
        {
            ComponentsToWorld = new HECSList<ComponentProvider<T>>();
        }

        public ComponentProvider(World world)
        {
            World = world;
            world.RegisterComponentProvider(this);
            check = (T)TypesMap.GetComponentFromFactory(TypeIndex);
        }

        internal override int TypeIndexProvider => TypeIndex;

        public int Priority { get; } = -2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T AddComponent(int entityIndex)
        {
            Components[entityIndex] = (T)TypesMap.GetComponentFromFactory(TypeIndex);
            Add(entityIndex);
            return Components[entityIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T AddComponent(int index, in T component)
        {
            Components[index] = component;
            Add(index);
            return this.Components[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent(int index)
        {
            return ref Components[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetOrAddComponent(int index)
        {
            if (Has(index))
                return Components[index];

            if (Components[index] != null)
            {
                Components[index].IsAlive = true;
                return AddComponent(index, Components[index]);
            }

            return AddComponent(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetOrAddComponent(int index, T component)
        {
            if (Has(index))
                return Components[index];

            return AddComponent(index, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetComponent(int index, out T component)
        {
            var entity = World.Entities[index];
            component = Components[index];
            return entity.Components.Contains(TypeIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void RemoveComponent(int entity)
        {
            Remove(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Has(int index)
        {
            return World.Entities[index].Components.Contains(TypeIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Remove(int index)
        {
            RegisterComponent(index, false);

            if (Components[index] is IDisposable disposable)
                disposable.Dispose();

            if (World.Entities[index].Components.Remove(TypeIndex))
                World.RegisterDirtyEntity(index);

            Components[index].IsAlive = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(int entityIndex)
        {
            World.Entities[entityIndex].Components.Add(TypeIndex);
            World.RegisterDirtyEntity(entityIndex);
            RegisterComponent(entityIndex, true);
        }

        public void Dispose()
        {
            Array.Clear(Components, 0, Components.Length);
            World = null;
        }

        public override void Resize()
        {
            Array.Resize(ref Components, Components.Length * 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void RegisterComponent(int entityIndex, bool add)
        {
            if (Components[entityIndex] is IWorldSingleComponent singleComponent)
                World.AddSingleWorldComponent(singleComponent, add);

            foreach (var ul in universalReactGlobals)
                ul.Value.React(Components[entityIndex], add);

            if (localListeners.ContainsKey(entityIndex))
            {
                if (add)
                {
                    addLocalComponent.Enqueue((entityIndex, Components[entityIndex], add));
                    IsDirty = true;
                }
                else
                {
                    foreach (var listener in localListeners[entityIndex])
                    {
                        listener.ComponentReact(Components[entityIndex], true);
                    }
                }
            }

            if (add)
            {
                IsDirty = true;
                addedComponent.Enqueue(Components[entityIndex]);
            }
            else
            {
                foreach (var r in reactComponentGlobals)
                    r.ComponentReactGlobal(Components[entityIndex], add);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void AddComponent(int entityIndex, IComponent component)
        {
            AddComponent(entityIndex, (T)component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IComponent GetIComponent(int entityIndex)
        {
            return Components[entityIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool SetIComponent(int entityIndex, IComponent component)
        {
            if (component is T needed)
            {
                Components[entityIndex] = needed;
                return true;
            }

            return false;
        }

        public override bool IsNeededType<Type>()
        {
            return check is Type;
        }

        public override void AddGlobalComponentListener<Component>(IReactComponentGlobal<Component> reactComponentGlobal, bool add)
        {
            if (add)
                this.reactComponentGlobals.Add(reactComponentGlobal as IReactComponentGlobal<T>);
            else
                this.reactComponentGlobals.Remove(reactComponentGlobal as IReactComponentGlobal<T>);
        }

        public void AddLocalComponentListener(int entityIndex, IReactComponentLocal<T> reactComponentLocal, bool add)
        {
            if (localListeners.TryGetValue(entityIndex, out var listeners))
            {
                if (add)
                    listeners.Add(reactComponentLocal);
                else
                    listeners.Remove(reactComponentLocal);
            }
        }

        public override void AddGlobalUniversalListener<Component>(IReactGenericGlobalComponent<Component> reactComponentGlobal, bool add)
        {
            var key = typeof(Component);

            if (universalReactGlobals.TryGetValue(key, out var reactive))
                (reactive as UniversalReactGlobalT<Component>).AddListener(reactComponentGlobal, add);
            else
            {
                var react = new UniversalReactGlobalT<Component>(World);
                react.AddListener(reactComponentGlobal, add);
                universalReactGlobals.Add(key, react);
            }
        }

        public void ForceReact()
        {
            PriorityUpdateLocal();

            foreach (var ur in universalReactGlobals)
                ur.Value.ForceReact();
        }

        public void PriorityUpdateLocal()
        {
            if (IsDirty)
            {
                while (addedComponent.TryDequeue(out var component))
                {
                    foreach (var r in reactComponentGlobals)
                        r.ComponentReactGlobal(component, true);
                }

                while(addLocalComponent.TryDequeue(out var component))
                {
                    if (localListeners.TryGetValue(component.Item1, out var listeners))
                    {
                        foreach (var l in listeners)
                        {
                            l.ComponentReact(component.Item2, component.Item3);
                        }
                    }
                }

                IsDirty = false;
            }
        }
    }

    public abstract partial class ComponentProvider
    {
        internal abstract int TypeIndexProvider { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void RemoveComponent(int entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool Has(int index);
        public abstract void Resize();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void AddComponent(int entityIndex, IComponent component);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract IComponent GetIComponent(int entityIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool SetIComponent(int entityIndex, IComponent component);

        public abstract void RegisterComponent(int entityIndex, bool add);
        public abstract bool IsNeededType<Type>();

        public abstract void AddGlobalComponentListener<Component>(IReactComponentGlobal<Component> reactComponentGlobal, bool add) where Component : IComponent;
        public abstract void AddGlobalUniversalListener<Component>(IReactGenericGlobalComponent<Component> reactComponentGlobal, bool add);
    }
}
