using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public sealed partial class ComponentProvider<T> : ComponentProvider, IPriorityUpdatable, IDisposable where T : IComponent
    {
        public static HECSList<ComponentProvider<T>> ComponentsToWorld = new HECSList<ComponentProvider<T>>(16);
        public T[] Components = new T[World.StartEntitiesCount];
        public World World;
        public static int TypeIndex = IndexGenerator.GetIndexForType(typeof(T));
        private HECSList<IReactComponentGlobal<T>> reactComponentGlobals = new HECSList<IReactComponentGlobal<T>>(2);
        private Dictionary<Type, UniversalReact> universalReactGlobals = new Dictionary<Type, UniversalReact>(0);
        private Dictionary<int, HECSList<IReactComponentLocal<T>>> localListeners = new Dictionary<int, HECSList<IReactComponentLocal<T>>>(0);
        private Dictionary<int, Dictionary<Type, UniversalReact>> localGenericListeners = new Dictionary<int, Dictionary<Type, UniversalReact>>(0);

        private Queue<T> addedComponent = new Queue<T>(4);
        private Queue<(int, bool)> addLocalComponent = new Queue<(int, bool)>(1);
        
        private HECSList<int> addedGenericLocal = new HECSList<int>(1);
        private HECSList<int> addedGenericGlobal = new HECSList<int>(1);

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
            if (Components[index] != null && Components[index].IsAlive)
                Remove(index);

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
                Add(index);
                return Components[index];
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
            if (!Has(index)) 
                return;

            if (World.Entities[index].Components.Remove(TypeIndex))
                World.RegisterDirtyEntity(index);

            if (Components[index] == null || !Components[index].IsAlive)
                return;

            RegisterComponent(index, false);

            var component = Components[index];

            if (component is IDisposable disposable)
                disposable.Dispose();

            Components[index].IsAlive = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(int entityIndex)
        {
            World.Entities[entityIndex].Components.Add(TypeIndex);
            World.RegisterDirtyEntity(entityIndex);
            RegisterComponent(entityIndex, true);
        }

        public override void Dispose()
        {
            World.GlobalUpdateSystem.Register(this, false);
            for (int i = 0; i < Components.Length; i++)
            {
                Components[i] = default;
            }
            World = null;
        }

        public override void Resize()
        {
            Array.Resize(ref Components, Components.Length * 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void RegisterComponent(int entityIndex, bool add)
        {
            var component = Components[entityIndex];
            World.AdditionalProcessing(component, World.Entities[entityIndex], add);

            if (add)
            {
                component.Owner = World.Entities[entityIndex];
                component.IsAlive = true;
            }
            else
            {
                component.IsAlive = false;
            }

            if (add && !World.Entities[entityIndex].IsInited)
                return;

            if (add)
            {
                if (Components[entityIndex] is IInitable initable)
                    initable.Init();

                if (Components[entityIndex] is IAfterEntityInit afterInit)
                    afterInit.AfterEntityInit();
            }

            if (Components[entityIndex] is IWorldSingleComponent singleComponent)
                World.AddSingleWorldComponent(singleComponent, add);

            RegisterReactive(entityIndex, add);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void RegisterReactive(int entityIndex, bool add)
        {
            if (!isReactive)
                return;

            if (isGenericReactive)
            {
                if (!add)
                {
                    foreach (var ul in universalReactGlobals)
                        ul.Value.React(Components[entityIndex], add);

                    addedGenericGlobal.Remove(entityIndex);
                }

                else
                    addedGenericGlobal.Add(entityIndex);
            }

            if (isGenericReactiveLocal)
            {
                if (!add)
                {
                    if (localGenericListeners.TryGetValue(entityIndex, out var localListeners))
                        foreach (var l in localListeners)
                            l.Value.React(Components[entityIndex], add);
                    
                    addedGenericLocal.Remove(entityIndex);
                }
                else
                    addedGenericLocal.Add(entityIndex);
            }

            if (isReactiveLocal || isGenericReactiveLocal)
            {
                if (localListeners.ContainsKey(entityIndex))
                {
                    if (add)
                    {
                        addLocalComponent.Enqueue((entityIndex, add));
                    }
                    else
                    {
                        var list = localListeners[entityIndex];
                        var count = list.Count;

                        for (int i = 0; i < count; i++)
                            list.Data[i].ComponentReact(Components[entityIndex], false);
                    }
                }
            }

            if (isReactiveGlobal)
            {
                if (add)
                {
                    addedComponent.Enqueue(Components[entityIndex]);
                }
                else
                {
                    var count = reactComponentGlobals.Count;
                    for (int i = 0; i < count; i++)
                        reactComponentGlobals.Data[i].ComponentReactGlobal(Components[entityIndex], add);
                }
            }

            if (add)
                isDirty = true;
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
                this.reactComponentGlobals.RemoveSwap(reactComponentGlobal as IReactComponentGlobal<T>);

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
                    listeners.RemoveSwap(reactComponentLocal);
            }
            else
            {
                if (add)
                {
                    localListeners.Add(entityIndex, new HECSList<IReactComponentLocal<T>>(8));
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
        }

        public void PriorityUpdateLocal()
        {
            if (isDirty)
            {
                while (addedComponent.TryDequeue(out var component))
                {
                    if (!component.IsAlive())
                        continue;

                    var reactComponentGlobalsCount = reactComponentGlobals.Count;

                    for (int i = 0; i < reactComponentGlobalsCount; i++)
                        reactComponentGlobals.Data[i].ComponentReactGlobal(component, true);
                }


                var countOfAddededGeneric = addedGenericGlobal.Count;
                for (int i = 0; i < countOfAddededGeneric; i++)
                {
                        foreach (var ur in universalReactGlobals.Values)
                        ur.React(Components[addedGenericGlobal.Data[i]], true);
                }

                while (addLocalComponent.TryDequeue(out var component))
                {
                    if (!Components[component.Item1].IsAlive())
                        continue;

                    if (localListeners.TryGetValue(component.Item1, out var listeners))
                    {
                        var listenersCount = listeners.Count;

                        for (int i = 0; i < listenersCount; i++)
                            listeners.Data[i].ComponentReact(Components[component.Item1], component.Item2);
                    }

                    if (isGenericReactiveLocal)
                        if (localGenericListeners.TryGetValue(component.Item1, out var universallisteners))
                        {
                            foreach (var ur in universallisteners.Values)
                                ur.React(Components[component.Item1], true);
                        }
                }

                var countOfAddededLocalGeneric = addedGenericLocal.Count;
                for (int i = 0; i < countOfAddededLocalGeneric; i++)
                {
                    if (localGenericListeners.TryGetValue(addedGenericLocal.Data[i], out var universallisteners))
                    {
                        foreach (var ur in universallisteners.Values)
                            ur.React(Components[addedGenericLocal.Data[i]], true);
                    }
                }

                if (countOfAddededGeneric != 0)
                    addedGenericGlobal.Clear();
                
                if (countOfAddededLocalGeneric != 0)
                    addedGenericLocal.Clear();
                
                isDirty = false;
            }
        }
    }

    public abstract partial class ComponentProvider : IDisposable
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

        /// <summary>
        /// this is for Entity init puprose, do not use it manualy
        /// </summary>
        /// <param name="entityIndex"></param>
        /// <param name="add"></param>
        public abstract void RegisterReactive(int entityIndex, bool add);
        public abstract bool IsNeededType<Type>();

        public abstract void AddGlobalComponentListener<Component>(IReactComponentGlobal<Component> reactComponentGlobal, bool add) where Component : IComponent;
        public abstract void AddGlobalGenericListener<Component>(IReactGenericGlobalComponent<Component> reactComponentGlobal, bool add);
        public abstract void AddLocalGenericListener<Component>(int index, IReactGenericLocalComponent<Component> reactComponentGlobal, bool add);
        public abstract void Dispose();
    }
}
