using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public sealed partial class EntitiesFilter : IEquatable<EntitiesFilter>
    {
        private readonly World world;
        private HashSet<int> check = new HashSet<int>(512);
        private HECSList<int> entities = new HECSList<int>(512);
        private HECSList<int> include = new HECSList<int>(4);
        private HECSList<int> exclude = new HECSList<int>(4);

        public bool IsNeedFullUpdate;
        
        private int includeHash;
        private int excludeHash;

        public int IncludeHash { get => includeHash;}
        public int ExcludeHash { get => excludeHash; }

        public int Count => entities.Count;
        public ref int[] Entities => ref entities.Data;

        internal EntitiesFilter(World world, Filter include, Filter exclude)
        {
            this.world = world;
            include.AddToHashSet(this.include);
            exclude.AddToHashSet(this.exclude);
            
            includeHash = includeHash.GetHashCode();
            excludeHash = excludeHash.GetHashCode();
        }

        internal EntitiesFilter(World world, Filter include)
        {
            this.world = world;
            include.AddToHashSet(this.include);
            includeHash = includeHash.GetHashCode();
            world.RegisterEntityFilter(this);
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
        internal void UpdateFilter(int[] updatedEntities, int lenght)
        {
            for (int i = 0; i < lenght; i++)
            {
                ref var currentEntity = ref world.Entities[updatedEntities[i]];

                if (!currentEntity.IsAlive)
                {
                    check.Remove(currentEntity.Index);
                    continue;
                }

                for (int z = 0; z < include.Count; z++)
                {
                    if (!currentEntity.Components.Contains(include.Data[z]))
                    {
                        check.Remove(currentEntity.Index);
                        goto exit;
                    }
                }

                for (int x = 0; x < exclude.Count; x++)
                {
                    if (currentEntity.Components.Contains(exclude.Data[x]))
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

        public void ForceUpdateFilter()
        {
            world.ForceUpdateFilter(this);
        }

        public override bool Equals(object obj)
        {
            return obj is EntitiesFilter filter && filter.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            return includeHash + excludeHash;
        }

        public bool Equals(EntitiesFilter other)
        {
            return other.GetHashCode() == GetHashCode();
        }

        public ref struct Enumerator
        {
            readonly int[] entities;
            readonly int count;
            private int currentStep;
            private IEntity[] fastEntities;

            public Enumerator(EntitiesFilter filter)
            {
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