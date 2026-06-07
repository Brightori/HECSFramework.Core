using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Local, "Target", "here we can hold identifier + alive entities for different scenarios when we need diff targets")]
    public sealed class CompositeTargetHolderComponent : BaseComponent, IDisposable
    {
        public HECSList<IDToTarget> Targets;

        public AliveEntity GetFirstOrDefault(int id)
        {
            for (int i = 0; i < Targets.Count; i++)
            {
                if (Targets[i].Id == id)
                    return Targets[i].AliveEntity;
            }

            return new AliveEntity();
        }

        public void AddTarget(Entity entity, int id)
        {
            Targets.Add(new IDToTarget { AliveEntity = entity, Id = id });
        }

        public void Remove(int id)
        {
            for (int i = 0; i < Targets.Count; i++)
            {
                if (Targets[i].Id == id)
                {
                    Targets.RemoveAtSwap(i);
                    return;
                }
            }
        }

        public void RemoveAll(int id)
        {
        start:

            for (int i = 0; i < Targets.Count; i++)
            {
                if (Targets[i].Id == id)
                {
                    Targets.RemoveAtSwap(i);
                    goto start;
                }
            }
        }

        public void Dispose()
        {
            Targets.Clear();
        }
    }

    public struct IDToTarget : IEquatable<IDToTarget>
    {
        public int Id;
        public AliveEntity AliveEntity;

        public override bool Equals(object obj)
        {
            return obj is IDToTarget target &&
                   Id == target.Id &&
                   AliveEntity.Equals(target.AliveEntity);
        }

        public bool Equals(IDToTarget other)
        {
            return Id == other.Id &&
                  AliveEntity.Equals(other.AliveEntity);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, AliveEntity);
        }
    }
}