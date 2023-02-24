using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial interface IData { }

namespace HECSFramework.Core
{
    public partial class World
    {
        private HECSList<ushort> updatedEntities = new HECSList<ushort>(32);
        private Dictionary<int, FastComponentProvider> fastComponentProvidersByTypeIndex = new Dictionary<int, FastComponentProvider>(256);
        private HECSList<FastEntitiesFilter> filters = new HECSList<FastEntitiesFilter>(16);

        public bool FastEntitiesIsDirty;

        public FastEntity[] FastEntities = new FastEntity[32];
        private Queue<ushort> freeEntities = new Queue<ushort>(32);

        private TypeRegistrator[] typeRegistrators = new TypeRegistrator[0];

        partial void InitFastWorld()
        {
            FillTypeRegistrators();

            for (int i = 1; i < FastEntities.Length; i++)
            {
                CreateNewEntity(i);
            }

            foreach (var t in typeRegistrators)
                t.RegisterWorld(this);

            GlobalUpdateSystem.FinishUpdate += UpdateFilters;
        }

        private void CreateNewEntity(int i)
        {
            ref var fast = ref FastEntities[i];
            fast.World = this;
            fast.ComponentIndeces = new HashSet<int>(8);
            fast.Index = (ushort)i;
            freeEntities.Enqueue((ushort)i);
        }

        partial void FillTypeRegistrators();

        public ref FastEntity GetFastEntity()
        {
            if (freeEntities.TryDequeue(out var index))
            {
                ref var fastEntity = ref FastEntities[index];
                fastEntity.IsReady = true;
                RegisterUpdatedFastEntity(ref fastEntity);
                return ref FastEntities[index];
            }

            return ref ResizeAndReturn();
        }

        public void DestroyFastEntity(ushort index)
        {
            ref var fastEntity = ref FastEntities[index];
            fastEntity.IsReady = false;
            fastEntity.Generation++;
            fastEntity.ComponentIndeces.Clear();
            freeEntities.Enqueue(index);
            RegisterUpdatedFastEntity(ref fastEntity);
        }

        private ref FastEntity ResizeAndReturn()
        {
            var currentLenght = FastEntities.Length;
            Array.Resize(ref FastEntities, currentLenght*2);

            for (int i = currentLenght; i < FastEntities.Length; i++)
            {
                if (!FastEntities[i].IsReady)
                {
                    CreateNewEntity(i);
                }
            }

            foreach (var p in fastComponentProvidersByTypeIndex)
            {
                p.Value.Resize();
            }

            return ref GetFastEntity();
        }

        public FastComponentProvider GetFastComponentProvider(int typeIndex)
        {
            return fastComponentProvidersByTypeIndex[typeIndex];
        }

        public void RegisterProvider(FastComponentProvider componentProvider)
        {
            fastComponentProvidersByTypeIndex.Add(componentProvider.TypeIndexProvider, componentProvider);
        }

        public FastEntitiesFilter GetFastFilter()
        {
            var filter = new FastEntitiesFilter(this);
            filters.Add(filter);
            return filter;
        }

        private void UpdateFilters()
        {
            foreach (var filter in filters)
            {
                if (filter.IsNeedFullUpdate)
                {
                    filter.UpdateFilter(updatedEntities.Data, updatedEntities.Count);
                    filter.IsNeedFullUpdate = false;
                }
                else
                    filter.UpdateFilter(updatedEntities.Data, updatedEntities.Count);
            }

            foreach (var e in updatedEntities)
                FastEntities[e].Updated = false;

            updatedEntities.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterUpdatedFastEntity(ushort index)
        {
            ref var fastEntity = ref FastEntities[index];

            if (fastEntity.Updated)
                return;

            fastEntity.Updated = true;
            updatedEntities.Add(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RegisterUpdatedFastEntity(ref FastEntity fastEntity)
        {
            if (fastEntity.Updated)
                return;

            fastEntity.Updated = true;
            updatedEntities.Add(fastEntity.Index);
        }

        partial void FastWorldDispose()
        {
            GlobalUpdateSystem.FinishUpdate -= UpdateFilters;

            foreach (var t in typeRegistrators)
                t.UnRegisterWorld(this);

            fastComponentProvidersByTypeIndex.Clear();
        }
    }
}
