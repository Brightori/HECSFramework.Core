using System;
using System.Linq;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Counters, "System for operating counters on this entity, process changes of values and add|remove modifiers to modifiable counters")]
    public sealed partial class CountersHolderSystem : BaseSystem, ICountersHolderSystem, IReactComponentLocal
    {
        [Required]
        public CountersHolderComponent countersHolder;

        public override void InitSystem()
        {
        }

        public void CommandReact(AddCounterModifierCommand<float> command)
        {
            if (countersHolder.TryGetCounter<ICounterModifiable<float>>(command.Id, out var counter))
            {
                if (command.IsUnique)
                    counter.AddUniqueModifier(command.Owner, command.Modifier);
                else
                    counter.AddModifier(command.Owner, command.Modifier);
            }
        }

        public void CommandReact(RemoveCounterModifierCommand<float> command)
        {
            if (countersHolder.TryGetCounter<ICounterModifiable<float>>(command.Id, out var counter))
                counter.RemoveModifier(command.Owner, command.Modifier);
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

        public void CommandReact(AddCounterModifierBySubIDCommand<float> command)
        {
            foreach (var c in countersHolder.Counters)
            {
                if (c.Value is ISubCounter subCounter && c.Value is ICounterModifiable<float> modifiable)
                {
                    if (subCounter.Id == command.Id)
                    {
                        if (command.IsUnique)
                            modifiable.AddUniqueModifier(command.Owner, command.Modifier);
                        else
                            modifiable.AddModifier(command.Owner, command.Modifier);
                    }
                }
            }
        }
    }

    public interface ICountersHolderSystem : ISystem,
        IReactCommand<AddCounterModifierCommand<float>>,
        IReactCommand<AddCounterModifierBySubIDCommand<float>>,
        IReactCommand<RemoveCounterModifierCommand<float>>,
        IReactCommand<ResetCountersCommand>
    {
    }
}