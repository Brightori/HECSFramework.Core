using System;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    internal sealed partial class ComponentProvider<T> : ComponentProvider, IDisposable where T : IComponent, new()
    {
        public static HECSList<ComponentProvider<T>> ComponentsToWorld = new HECSList<ComponentProvider<T>>(16);
        public T[] Components = new T[256];
        public World World;
        public static int TypeIndex = IndexGenerator.GetIndexForType(typeof(T));

        static ComponentProvider()
        {
            ComponentsToWorld = new HECSList<ComponentProvider<T>>();
        }

        public ComponentProvider(World world)
        {
            World = world;
            world.RegisterComponentProvider(this);
        }

        internal override int TypeIndexProvider => TypeIndex;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T AddComponent(int entityIndex)
        {
            Components[entityIndex] = new T();
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
            {
                Components[index].IsAlive = true;
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
        private void Remove(int fastEntityIndex)
        {
            if (World.FastEntities[fastEntityIndex].ComponentIndeces.Remove(TypeIndex))
                World.RegisterDirtyEntity(fastEntityIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(int entityIndex)
        {
            World.Entities[entityIndex].Components.Add(TypeIndex);
            World.RegisterDirtyEntity(entityIndex);
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
            World.Entities[entityIndex].RegisterComponentListenersService.Invoke(Components[entityIndex], add);
            World.GlobalComponentListenerService.Invoke(Components[entityIndex], add);

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

        public abstract void RegisterComponent(int entityIndex, bool add);
    }
}
