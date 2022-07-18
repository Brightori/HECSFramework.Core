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
    }

    public interface ICountersHolderSystem : ISystem,
        IReactCommand<AddCounterModifierCommand<float>>,
        IReactCommand<RemoveCounterModifierCommand<float>>,
        IReactCommand<ResetCountersCommand>
    {
    }
}