using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Counters, "System for operating counters on this entity, process changes of values and add|remove modifiers to modifiable counters")]
    public sealed partial class CountersHolderSystem : BaseSystem, ICountersHolderSystem, IReactGenericLocalComponent<ICounter>
    {
        [Required]
        public CountersHolderComponent countersHolder;

        public Guid ListenerGuid { get; } = Guid.NewGuid();

        public override void InitSystem()
        {
            foreach (var c in Owner.GetComponentsByType<ICounter>())
                countersHolder.AddCounter(c);
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

        public void CommandReact(AddCounterModifierBySubIDCommand<float> command)
        {
            foreach (var c in countersHolder.Counters)
            {
                if (c.Value is ISubCounter subCounter && c.Value is ICounterModifiable<float> modifiable)
                {
                    if (subCounter.SubId == command.Id)
                    {
                        if (command.IsUnique)
                            modifiable.AddUniqueModifier(command.Owner, command.Modifier);
                        else
                            modifiable.AddModifier(command.Owner, command.Modifier);
                    }
                }
            }
        }

        public void CommandReact(AddCounterModifierCommand<int> command)
        {
            if (countersHolder.TryGetCounter<ICounterModifiable<int>>(command.Id, out var counter))
            {
                if (command.IsUnique)
                    counter.AddUniqueModifier(command.Owner, command.Modifier);
                else
                    counter.AddModifier(command.Owner, command.Modifier);
            }
        }

        public void CommandReact(RemoveCounterModifierCommand<int> command)
        {
            if (countersHolder.TryGetCounter<ICounterModifiable<int>>(command.Id, out var counter))
                counter.RemoveModifier(command.Owner, command.Modifier);
        }

        public void ComponentReactLocal(ICounter component, bool isAdded)
        {
            if (isAdded)
                countersHolder.AddCounter(component);
            else
                countersHolder.RemoveCounter(component);
        }
    }

    public interface ICountersHolderSystem : ISystem,
        IReactCommand<AddCounterModifierCommand<float>>,
        IReactCommand<AddCounterModifierCommand<int>>,
        IReactCommand<AddCounterModifierBySubIDCommand<float>>,
        IReactCommand<RemoveCounterModifierCommand<float>>,
        IReactCommand<RemoveCounterModifierCommand<int>>,
        IReactCommand<ResetCountersCommand>
    {
    }
}