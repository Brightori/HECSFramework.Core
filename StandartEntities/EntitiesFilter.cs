using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public sealed partial class EntitiesFilter : IEquatable<EntitiesFilter>
    {
        private readonly World world;
        private HashSet<int> check = new HashSet<int>(World.StartEntitiesCount);
        private HECSList<int> entities = new HECSList<int>(World.StartEntitiesCount);
        private HECSList<int> include = new HECSList<int>(4);
        private HECSList<int> exclude = new HECSList<int>(4);

        public bool IsNeedFullUpdate;
        
        private int includeHash;
        private int excludeHash;

        public int IncludeHash { get => includeHash;}
        public int ExcludeHash { get => excludeHash; }

        public int Count => entities.Count;
        public int[] Entities
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => entities.Data;
        }

      

        public Entity this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => world.Entities[entities.Data[index]];
        }

        internal EntitiesFilter(World world, Filter include, Filter exclude)
        {
            this.world = world;
            include.AddToHashSet(this.include);
            exclude.AddToHashSet(this.exclude);
            
            includeHash = include.GetHashCode();
            excludeHash = exclude.GetHashCode();
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

        public bool ExcludeMaskContains<T>() where T: IComponent
        {
            return exclude.Contains(ComponentProvider<T>.TypeIndex);
        }

        public bool IncludeMaskContains<T>() where T : IComponent
        {
            return include.Contains(ComponentProvider<T>.TypeIndex);
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
                entities.Add(entity);
        }

        public Entity FirstOrDefault(Func<Entity, bool> func)
        {
            foreach (var e in entities)
            {
                if (func(world.Entities[e]))
                    return world.Entities[e];
            }

            return null;
        }

        public Entity FirstOrDefault()
        {
            if (entities.Count > 0)
                return this[0];

            return null;
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

        public Entity[] ToArray()
        {
            var result = new Entity[entities.Count];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = this[i];
            }

            return result;
        }

        public struct Enumerator
        {
            readonly int[] entities;
            readonly int count;
            private int currentStep;
            private Entity[] currentEntities;

            public Enumerator(EntitiesFilter filter)
            {
                entities = filter.entities.Data;
                count = filter.entities.Count;
                currentStep = -1;
                currentEntities = filter.world.Entities;
            }

            public Entity Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => currentEntities[entities[currentStep]];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (++currentStep < count)
                {
                    if (currentEntities[entities[currentStep]].IsAlive)
                        return true;
                    else
                        return MoveNext();
                }
                return false;
            }
        }
    }
}