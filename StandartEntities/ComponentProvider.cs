using System;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public sealed partial class ComponentProvider<T> : ComponentProvider, IDisposable where T : IComponent
    {
        public static HECSList<ComponentProvider<T>> ComponentsToWorld = new HECSList<ComponentProvider<T>>(16);
        public T[] Components = new T[World.StartEntitiesCount];
        public World World;
        public static int TypeIndex = IndexGenerator.GetIndexForType(typeof(T));
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
                Components[entityIndex].Init();
                Components[entityIndex].AfterInit();
            }

            if (Components[entityIndex] is IWorldSingleComponent singleComponent)
                World.AddSingleWorldComponent(singleComponent, add);
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
        public abstract bool IsNeededType<Type>();

        public abstract void Dispose();
    }
}
