using System;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    internal sealed partial class ComponentProvider<T> : ComponentProvider, IDisposable where T : struct, IData
    {
        public static ConcurrencyList<ComponentProvider<T>> ComponentsToWorld = new ConcurrencyList<ComponentProvider<T>>(16);
        public T[] Components = new T[1024];
        public World World;
        public int TypeIndex = IndexGenerator.GetIndexForType(typeof(T));

        static ComponentProvider()
        {
            ComponentsToWorld = new ConcurrencyList<ComponentProvider<T>>();
        }

        public ComponentProvider(World world)
        {
            World = world;
            world.RegisterProvider(this);
        }

        internal override int TypeIndexProvider => TypeIndex;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent(ushort index)
        {
            Components[index] = new T();
            Add(index);
            return ref Components[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent(ushort index, in T component)
        {
            Components[index] = component;
            Add(index);
            return ref this.Components[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent(ushort index)
        {
            return ref Components[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetOrAddComponent(ushort index)
        {
            if (Has(index))
                return ref Components[index];

            return ref AddComponent(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetOrAddComponent(ushort index, in T component)
        {
            if (Has(index))
                return ref Components[index];

            return ref AddComponent(index, component);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T TryGetComponent(ushort index, out bool exist)
        {
            ref var fastEntity = ref World.FastEntities[index];

            if (fastEntity.ComponentIndeces.Contains(TypeIndex))
                exist = true;
            else
                exist = false;

            return ref Components[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void RemoveComponent(ushort entity)
        {
            Remove(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Has(ushort index)
        {
            return World.FastEntities[index].ComponentIndeces.Contains(TypeIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Remove(ushort fastEntityIndex)
        {
            if (World.FastEntities[fastEntityIndex].ComponentIndeces.Remove(TypeIndex))
                World.RegisterUpdatedFastEntity(fastEntityIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(ushort fastEntityIndex)
        {
            World.FastEntities[fastEntityIndex].ComponentIndeces.Add(TypeIndex);
            World.RegisterUpdatedFastEntity(fastEntityIndex);
        }

        public void Dispose()
        {
            Array.Clear(Components, 0, Components.Length);
            World = null;
        }
    }

    public abstract partial class ComponentProvider
    {
        internal abstract int TypeIndexProvider { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void RemoveComponent(ushort entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool Has(ushort index);
    }
}
