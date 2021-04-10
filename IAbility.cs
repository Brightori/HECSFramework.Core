﻿using HECSFramework.Core.Helpers;
using System.Collections.Generic;
using Systems;

namespace HECSFramework.Core
{
    public interface IAbility : IEntity
    {
        void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }

    public class Ability : Entity, IAbility
    {
        private List<IExecuteAbilitySystem> executeAbilitySystems = new List<IExecuteAbilitySystem>(2);

        public Ability(string id, int worldIndex) : base(id, worldIndex)
        {
        }

        public override void AddHecsSystem<T>(T system)
        {
            base.AddHecsSystem(system);

            if (system is IExecuteAbilitySystem abilitySystem)
                executeAbilitySystems.AddOrRemoveElement(abilitySystem, true);
        }

        public override void RemoveHecsSystem(ISystem system)
        {
            base.RemoveHecsSystem(system);
            
            if (system is IExecuteAbilitySystem abilitySystem)
                executeAbilitySystems.AddOrRemoveElement(abilitySystem, false);
        }

        public void Execute(IEntity owner = null, IEntity target = null, bool enable = true)
        {
            foreach (var a in executeAbilitySystems)
                a.Execute(owner, target, enable);
        }
    }
}