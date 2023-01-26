using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    [Serializable]
    public struct FastEntity
    {
        public World World;
        public ushort Index;
        public ushort Generation;
        public bool IsReady;
        public bool Updated;
        public HashSet<int> ComponentIndeces;

        public static ref FastEntity Get(World world = null)
        {
            if (world == null)
                world = EntityManager.Default;
            return ref world.GetFastEntity();
        }

        public void DestroyFastEntity()
        {
            World.DestroyFastEntity(Index);
        }
    }

    public static partial class FastEntityExtentions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFastEntityAlive(this FastEntity fastEntity)
        {
            return fastEntity.IsReady && fastEntity.World.FastEntities[fastEntity.Index].Generation == fastEntity.Generation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetComponent<T>(this FastEntity fastEntity) where T : struct, IFastComponent
        {
            return ref FastComponentProvider<T>.ComponentsToWorld.Data[fastEntity.World.Index].GetComponent(fastEntity.Index);
        }
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetOrAddComponent<T>(this FastEntity fastEntity) where T : struct, IFastComponent
        {
            return ref FastComponentProvider<T>.ComponentsToWorld.Data[fastEntity.World.Index].GetOrAddComponent(fastEntity.Index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetOrAddComponent<T>(this FastEntity fastEntity, in T component) where T : struct, IFastComponent
        {
            return ref FastComponentProvider<T>.ComponentsToWorld.Data[fastEntity.World.Index].GetOrAddComponent(fastEntity.Index, component);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this FastEntity fastEntity) where T : struct, IFastComponent
        {
            return ref FastComponentProvider<T>.ComponentsToWorld.Data[fastEntity.World.Index].AddComponent(fastEntity.Index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this FastEntity fastEntity, in T component) where T : struct, IFastComponent
        {
            return ref FastComponentProvider<T>.ComponentsToWorld.Data[fastEntity.World.Index].AddComponent(fastEntity.Index, in component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T TryGetComponent<T>(this FastEntity fastEntity, out bool exists) where T : struct, IFastComponent
        {
            return ref FastComponentProvider<T>.ComponentsToWorld.Data[fastEntity.World.Index].TryGetComponent(fastEntity.Index, out exists);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent<T>(this FastEntity fastEntity) where T : struct, IFastComponent
        {
            FastComponentProvider<T>.ComponentsToWorld.Data[fastEntity.World.Index].RemoveComponent(fastEntity.Index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent(this FastEntity fastEntity, int typeIndex) 
        {
            fastEntity.World.GetFastComponentProvider(typeIndex).RemoveComponent(fastEntity.Index);
        }
    }
}