using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public sealed partial class FastEntitiesFilter
    {
        private readonly World world;
        private HashSet<ushort> check = new HashSet<ushort>(512);
        private HECSList<ushort> entities = new HECSList<ushort>(512);
        private HECSList<int> include = new HECSList<int>(4);
        private HECSList<int> exclude = new HECSList<int>(4);

        public bool IsNeedFullUpdate;

        public int Count => entities.Count;
        public ref ushort[] Entities => ref entities.Data;

        internal FastEntitiesFilter(World world)
        {
            this.world = world;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public World GetWorld()
        {
            return world;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UpdateFilter(ushort[] updatedEntities, int lenght)
        {
            for (int i = 0; i < lenght; i++)
            {
                ref var currentEntity = ref world.FastEntities[updatedEntities[i]];

                if (!currentEntity.IsReady)
                {
                    check.Remove(currentEntity.Index);
                    continue;
                }

                for (int z = 0; z < include.Count; z++)
                {
                    if (!currentEntity.ComponentIndeces.Contains(include.Data[z]))
                    {
                        check.Remove(currentEntity.Index);
                        goto exit;
                    }
                }

                for (int x = 0; x < exclude.Count; x++)
                {
                    if (currentEntity.ComponentIndeces.Contains(exclude.Data[x]))
                    {
                        check.Remove(currentEntity.Index);
                        goto exit;
                    }
                }

                check.Add(currentEntity.Index);
                //AddEntityToFilter(currentEntity);

            exit:;
            }

            entities.ClearFast();

            foreach (var entity in check)
            {
                entities.Add(entity);
            }
        }

        public FastEntitiesFilter With<T>() where T : struct, IFastComponent
        {
            var TIndex = FastComponentProvider<T>.TypeIndex;

            if (!include.Contains(TIndex))
                include.Add(TIndex);

            IsNeedFullUpdate = true;
            world.FastEntitiesIsDirty = true;
            return this;
        }

        public FastEntitiesFilter WithOut<T>() where T : struct, IFastComponent
        {
            var TIndex = FastComponentProvider<T>.TypeIndex;

            if (!exclude.Contains(TIndex))
                exclude.Add(TIndex);

            IsNeedFullUpdate = true;
            world.FastEntitiesIsDirty = true;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveEntityFromFilter(FastEntity fastEntity)
        {
            check.Remove(fastEntity.Index);
        }

        private void AddEntityToFilter(FastEntity fastEntity)
        {
            var neededIndex = fastEntity.Index;

            if (entities.Contains(neededIndex))
                return;

            entities.Add(neededIndex);
        }

        public ref struct Enumerator
        {
            readonly FastEntitiesFilter filter;
            readonly ushort[] entities;
            readonly int count;
            private int currentStep;
            private FastEntity[] fastEntities;

            public Enumerator(FastEntitiesFilter filter)
            {
                this.filter = filter;
                entities = filter.entities.Data;
                count = filter.entities.Count;
                currentStep = -1;
                fastEntities = filter.world.FastEntities;
            }

            public ref FastEntity Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref fastEntities[entities[currentStep]];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++currentStep < count;
            }
        }
    }
}