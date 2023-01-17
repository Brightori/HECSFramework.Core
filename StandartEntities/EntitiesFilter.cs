using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Codice.CM.Client.Differences;

namespace HECSFramework.Core
{
    public sealed class ArchiType : IEquatable<ArchiType>
    {
        private HashSet<int> check = new HashSet<int>(512);
        private HECSList<int> entities = new HECSList<int>(512);
        private HECSList<int> include = new HECSList<int>(4);
        private HECSList<int> exclude = new HECSList<int>(4);
        private World world;

        public int Count => entities.Count;
        public ref int[] Entities => ref entities.Data;

        public int IncludeHash { get; private set; }
        public int ExcludeHash { get; private set; }

        public override bool Equals(object obj)
        {
            return obj is ArchiType type &&
                   EqualityComparer<HECSList<int>>.Default.Equals(include, type.include) &&
                   EqualityComparer<HECSList<int>>.Default.Equals(exclude, type.exclude) &&
                   IncludeHash == type.IncludeHash &&
                   ExcludeHash == type.ExcludeHash;
        }

        public bool Equals(ArchiType other)
        {
            return other.IncludeHash == IncludeHash && other.ExcludeHash == ExcludeHash;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IncludeHash, ExcludeHash);
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
            exit:;
            }

            entities.ClearFast();

            foreach (var entity in check)
            {
                entities.Add(entity);
            }
        }
    }


    public sealed partial class EntitiesFilter : IDisposable
    {
        private readonly World world;
        public ArchiType ArchiType;



        public int Count => ArchiType.Count;
        public ref int[] Entities => ref ArchiType.Entities;

        internal EntitiesFilter(World world)
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

       

        public EntitiesFilter With<T>() where T : class, IComponent, new()
        {
            var TIndex = ComponentProvider<T>.TypeIndex;

            if (!include.Contains(TIndex))
                include.Add(TIndex);

            IsNeedFullUpdate = true;
            world.FastEntitiesIsDirty = true;
            return this;
        }

        public EntitiesFilter WithOut<T>() where T : struct, IData
        {
            var TIndex = FastComponentProvider<T>.ComponentsToWorld.Data[world.Index].TypeIndex;

            if (!exclude.Contains(TIndex))
                exclude.Add(TIndex);

            IsNeedFullUpdate = true;
            world.FastEntitiesIsDirty = true;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveEntityFromFilter(IEntity fastEntity)
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ref struct Enumerator
        {
            readonly EntitiesFilter filter;
            readonly int[] entities;
            readonly int count;
            private int currentStep;
            private IEntity[] fastEntities;

            public Enumerator(EntitiesFilter filter)
            {
                this.filter = filter;
                entities = filter.entities.Data;
                count = filter.entities.Count;
                currentStep = -1;
                fastEntities = filter.world.Entities;
            }

            public ref IEntity Current
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