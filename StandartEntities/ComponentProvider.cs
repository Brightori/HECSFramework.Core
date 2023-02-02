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
        private Dictionary<Type, UniversalReact> universalReactGlobals = new Dictionary<Type, UniversalReact>(8);
        private Dictionary<int, HashSet<IReactComponentLocal<T>>> localListeners = new Dictionary<int, HashSet<IReactComponentLocal<T>>>(4);
        private Dictionary<int, Dictionary<Type, UniversalReact>> localGenericListeners = new Dictionary<int, Dictionary<Type, UniversalReact>>(16);

        private Queue<T> addedComponent = new Queue<T>(4);
        private Queue<(int, bool)> addLocalComponent = new Queue<(int, bool)>(4);

        private bool isDirty;

        //here we try to avoid reacts if they didnt needed;
        private bool isReactive;

        private bool isReactiveGlobal;
        private bool isReactiveLocal;
        private bool isGenericReactive;
        private bool isGenericReactiveLocal;

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
            world.GlobalUpdateSystem.Register(this, true);
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
            //probably we should  check if component is alive and return null if component not alive
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
                Components[index].Owner = World.Entities[index];

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

            ref var component = ref Components[index];

            if (component is IDisposable disposable)
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
            World.GlobalUpdateSystem.Register(this, false);
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

            if (!isReactive)
                return;

            if (isGenericReactive)
            {
                foreach (var ul in universalReactGlobals)
                    ul.Value.React(Components[entityIndex], add);
            }

            if (isGenericReactiveLocal)
            {
                if (localGenericListeners.TryGetValue(entityIndex, out var localListeners))
                    foreach (var l in localListeners)
                        l.Value.React(Components[entityIndex], add);
            }

            if (isReactiveLocal)
            {
                if (localListeners.ContainsKey(entityIndex))
                {
                    if (add)
                    {
                        addLocalComponent.Enqueue((entityIndex, add));
                        isDirty = true;
                    }
                    else
                    {
                        foreach (var listener in localListeners[entityIndex])
                        {
                            listener.ComponentReact(Components[entityIndex], false);
                        }
                    }
                }
            }

            if (isReactiveGlobal)
            {
                if (add)
                {
                    isDirty = true;
                    addedComponent.Enqueue(Components[entityIndex]);
                }
                else
                {
                    foreach (var r in reactComponentGlobals)
                        r.ComponentReactGlobal(Components[entityIndex], add);
                }
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

            isReactiveGlobal = true;
            isReactive = true;
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
            else
            {
                if (add)
                {
                    localListeners.Add(entityIndex, new HashSet<IReactComponentLocal<T>>());
                    localListeners[entityIndex].Add(reactComponentLocal);
                }
            }

            isReactiveLocal = true;
            isReactive = true;
        }

        public override void AddGlobalGenericListener<Component>(IReactGenericGlobalComponent<Component> reactComponentGlobal, bool add)
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

            isReactive = true;
            isGenericReactive = true;
        }

        public override void AddLocalGenericListener<Component>(int index, IReactGenericLocalComponent<Component> reactComponentGlobal, bool add)
        {
            var key = typeof(Component);

            if (localGenericListeners.TryGetValue(index, out var listeners))
            {
                if (listeners.TryGetValue(key, out var reactive))
                    (reactive as UniversalReactLocalT<Component>).AddListener(reactComponentGlobal, add);
                else
                {
                    var react = new UniversalReactLocalT<Component>(World);
                    react.AddListener(reactComponentGlobal, add);
                    listeners.Add(key, react);
                }
            }
            else
            {
                localGenericListeners.Add(index, new Dictionary<Type, UniversalReact>(4));
                var reactContainer = new UniversalReactLocalT<Component>(World);
                reactContainer.AddListener(reactComponentGlobal, add);
                localGenericListeners[index].Add(key, reactContainer);
            }

            isReactive = true;
            isGenericReactiveLocal = true;
        }

        public void ForceReact()
        {
            PriorityUpdateLocal();

            foreach (var ur in universalReactGlobals)
                ur.Value.ForceReact();
        }

        public void PriorityUpdateLocal()
        {
            if (isDirty)
            {
                while (addedComponent.TryDequeue(out var component))
                {
                    foreach (var r in reactComponentGlobals)
                        r.ComponentReactGlobal(component, true);
                }

                while (addLocalComponent.TryDequeue(out var component))
                {
                    if (localListeners.TryGetValue(component.Item1, out var listeners))
                    {
                        foreach (var l in listeners)
                        {
                            l.ComponentReact(Components[component.Item1], component.Item2);
                        }
                    }
                }

                isDirty = false;
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

        /// <summary>
        /// u should provide entity index here, not type index
        /// </summary>
        /// <param name="entityIndex"></param>
        /// <param name="add"></param>
        public abstract void RegisterComponent(int entityIndex, bool add);
        public abstract bool IsNeededType<Type>();

        public abstract void AddGlobalComponentListener<Component>(IReactComponentGlobal<Component> reactComponentGlobal, bool add) where Component : IComponent;
        public abstract void AddGlobalGenericListener<Component>(IReactGenericGlobalComponent<Component> reactComponentGlobal, bool add);
        public abstract void AddLocalGenericListener<Component>(int index, IReactGenericLocalComponent<Component> reactComponentGlobal, bool add);
    }
}
