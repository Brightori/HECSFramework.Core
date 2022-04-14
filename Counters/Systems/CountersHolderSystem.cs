using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable][Documentation(Doc.Counters, "System for operating counters on this entity, process changes of values and add|remove modifiers to modifiable counters")]
    public sealed class CountersHolderSystem : BaseSystem, IReactCommand<AddCounterModifierCommand<float>>, IReactCommand<RemoveCounterModifierCommand<float>>, IReactCommand<ResetCountersCommand>
    {
        [Required]
        public CountersHolderComponent countersHolder;

        public override void InitSystem()
        {
            foreach(var c in Owner.GetComponentsByType<ICounter>())
            {
                countersHolder.Counters.Add(c.Id, c);

                if (c is ICounterModifiable<float> modifCounter)
                {
                    countersHolder.FloatCounters.Add(modifCounter.Id, modifCounter);
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
    }
}