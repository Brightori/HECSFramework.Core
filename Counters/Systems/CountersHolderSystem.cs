using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Counters, "System for operating counters on this entity, process changes of values and add|remove modifiers to modifiable counters")]
    public sealed class CountersHolderSystem : BaseSystem, ICountersHolderSystem, IReactComponentLocal
    {
        [Required]
        public CountersHolderComponent countersHolder;

        public override void InitSystem()
        {
            foreach (var c in Owner.GetComponentsByType<ICounter>())
            {
                countersHolder.AddCounter(c);

                if (c is ICounterModifiable<float> modifCounter)
                {
                    countersHolder.AddFloatModifiableCounter(modifCounter);
                }
            }
        }

        public void CommandReact(AddCounterModifierCommand<float> command)
        {
            if (command.IsUnique)
                countersHolder.FloatCounters[command.Id].AddUniqueModifier(command.Owner, command.Modifier);
            else
            {
                countersHolder.FloatCounters[command.Id].AddModifier(command.Owner, command.Modifier);
            }
        }

        public void CommandReact(RemoveCounterModifierCommand<float> command)
        {
            countersHolder.FloatCounters[command.Id].RemoveModifier(command.Owner, command.Modifier);
        }

        public void CommandReact(ResetCountersCommand command)
        {
            countersHolder.ResetCounters();
        }
      
        public void ComponentReactLocal(IComponent component, bool isAdded)
        {
            if (component is ICounter counter)
            {
                if (isAdded)
                    countersHolder.AddCounter(counter);
                else
                    countersHolder.RemoveCounter(counter);
            }
        }
    }

    public interface ICountersHolderSystem : ISystem,
        IReactCommand<AddCounterModifierCommand<float>>,
        IReactCommand<RemoveCounterModifierCommand<float>>,
        IReactCommand<ResetCountersCommand>
    {
    }
}