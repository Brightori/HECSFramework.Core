using System;
using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Core.Helpers;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Counters, "System for operating counters on this entity, process changes of values and add|remove modifiers to modifiable counters")]
    public sealed partial class CountersHolderSystem : BaseSystem, ICountersHolderSystem 
    {
        [Required]
        public CountersHolderComponent CountersHolder;

        [Required]
        public ModifiersHolderComponent ModifiersHolderComponent;

        public Guid ListenerGuid { get; } = Guid.NewGuid();

        public override void InitSystem()
        {
            foreach (var c in Owner.GetComponentsByType<ICounter>())
                CountersHolder.AddCounter(c);
        }

        public void CommandReact(AddCounterModifierCommand<float> command)
        {
            if (CountersHolder.TryGetCounter<ICounterModifiable<float>>(command.Id, out var counter))
            {
                if (command.IsUnique)
                {
                    ModifiersHolderComponent.FloatModifiers.AddUniqueElement(command.Modifier);
                    counter.AddUniqueModifier(command.Owner, command.Modifier);
                }
                else
                {
                    counter.AddModifier(command.Owner, command.Modifier);
                    ModifiersHolderComponent.FloatModifiers.Add(command.Modifier);
                }
            }
        }

        public void CommandReact(RemoveCounterModifierCommand<float> command)
        {
            if (CountersHolder.TryGetCounter<ICounterModifiable<float>>(command.Id, out var counter))
            {
                counter.RemoveModifier(command.Owner, command.Modifier);
                ModifiersHolderComponent.FloatModifiers.Remove(command.Modifier);
            }
        }

        public void CommandReact(ResetCountersCommand command)
        {
            CountersHolder.ResetCountersModifiers();
        }

        public void CommandReact(AddCounterModifierBySubIDCommand<float> command)
        {
            foreach (var c in CountersHolder.Counters)
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
            if (CountersHolder.TryGetCounter<ICounterModifiable<int>>(command.Id, out var counter))
            {
                if (command.IsUnique)
                {
                    ModifiersHolderComponent.IntModifiers.AddUniqueElement(command.Modifier);
                    counter.AddUniqueModifier(command.Owner, command.Modifier);
                }
                else
                {
                    ModifiersHolderComponent.IntModifiers.Add(command.Modifier);
                    counter.AddModifier(command.Owner, command.Modifier);
                }
            }
        }

        public void CommandReact(RemoveCounterModifierCommand<int> command)
        {
            if (CountersHolder.TryGetCounter<ICounterModifiable<int>>(command.Id, out var counter))
            {
                counter.RemoveModifier(command.Owner, command.Modifier);
                ModifiersHolderComponent.IntModifiers.Remove(command.Modifier);
            }
        }

        public void ComponentReactLocal(ICounter component, bool isAdded)
        {
            if (isAdded)
                CountersHolder.AddCounter(component);
            else
                CountersHolder.RemoveCounter(component);
        }

        public void CommandReact(ComponentReactByTypeCommand<ICounter> command)
        {
            ComponentReactLocal(command.Value, true);
        }

        public void CommandReact(RemoveComponentReactByTypeCommand<ICounter> command)
        {
            ComponentReactLocal(command.Value, false);
        }
    }

    public interface ICountersHolderSystem : ISystem,
        IReactCommand<AddCounterModifierCommand<float>>,
        IReactCommand<AddCounterModifierCommand<int>>,
        IReactCommand<AddCounterModifierBySubIDCommand<float>>,
        IReactCommand<RemoveCounterModifierCommand<float>>,
        IReactCommand<RemoveCounterModifierCommand<int>>,
        IReactCommand<ComponentReactByTypeCommand<ICounter>>,
        IReactCommand<RemoveComponentReactByTypeCommand<ICounter>>,
        IReactCommand<ResetCountersCommand>
    {
    }
}