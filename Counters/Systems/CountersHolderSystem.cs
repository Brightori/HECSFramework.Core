using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Counters, "System for operating counters on this entity, process changes of values and add|remove modifiers to modifiable counters")]
    public sealed class CountersHolderSystem : BaseSystem, ICountersHolderSystem
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
                    if (c is ISubCounter subCounter)
                    {
                        countersHolder.AddComplexFloatModifiableCounter((subCounter.Id, subCounter.SubId), modifCounter);
                    }
                    else
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

        public void CommandReact(AddComplexCounterModifierCommand<float> command)
        {
            var key = (command.Id, command.SubId);

            if (countersHolder.ComplexFloatCounters.ContainsKey(key))
            {
                if (command.IsUnique)
                    countersHolder.ComplexFloatCounters[key].AddUniqueModifier(command.Owner, command.Modifier);
                else
                {
                    countersHolder.ComplexFloatCounters[key].AddModifier(command.Owner, command.Modifier);
                }
            }
        }

        public void CommandReact(RemoveComplexCounterModifierCommand<float> command)
        {
            var key = (command.Id, command.SubId);

            if (countersHolder.ComplexFloatCounters.ContainsKey(key))
            {
                countersHolder.ComplexFloatCounters[key].RemoveModifier(command.Owner, command.Modifier);
            }
        }
    }

    public interface ICountersHolderSystem : ISystem,
        IReactCommand<AddCounterModifierCommand<float>>,
        IReactCommand<AddComplexCounterModifierCommand<float>>,
        IReactCommand<RemoveCounterModifierCommand<float>>,
        IReactCommand<RemoveComplexCounterModifierCommand<float>>,
        IReactCommand<ResetCountersCommand>
    {
    }
}