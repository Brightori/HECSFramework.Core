using System;
using System.Collections.Generic;
using Commands;
using HECSFramework.Core;
using HECSFramework.Core.Helpers;
using Helpers;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Abilities, "This component holds current|default abilities, we operate this throw abilitis system")]
    public sealed partial class AbilitiesHolderComponent : BaseComponent, IDisposable
    {
        [HideInInspectorCrossPlatform]
        //this is collection of abilities what active and can be execute
        public List<Entity> Abilities = new List<Entity>(2);

        //this is abilities what we have, but they not used now, for example - we can have 20 abilities but active in slots only 4
        public List<Entity> AvailableAbilities = new List<Entity>(2);

        //this collection help us execute abilities by id
        public Dictionary<int, Entity> IndexToAbility = new Dictionary<int, Entity>();
        public Dictionary<Guid, Entity> GuidToAbility = new Dictionary<Guid, Entity>();

        public void AddAvailableAbility(Entity ability)
        {
            ability.GetOrAddComponent<AbilityOwnerComponent>().AbilityOwner = Owner;
            AvailableAbilities.Add(ability);
        }

        public void AddAbility(Entity ability, bool needInit = false)
        {
            ability.GetOrAddComponent<AbilityOwnerComponent>().AbilityOwner = Owner;

            if (needInit && !ability.IsInited)
                ability.Init();

            Abilities.Add(ability);
            
            if (ability.ContainsMask<AbilityByGuidTagComponent>())
            {
                GuidToAbility.Add(ability.GUID, ability);
            }
            else
            {
                IndexToAbility.Add(ability.GetComponent<ActorContainerID>().ContainerIndex, ability);

                if (ability.TryGetComponent(out AdditionalAbilityIndexComponent component))
                {
                    foreach (var i in component.AdditionalIndeces)
                        IndexToAbility.AddOrReplace(i, ability);
                }
            }
        }

        public void AddPassiveAbility(Entity ability, Entity from, Entity to)
        {
            ability.GetOrAddComponent<AbilityOwnerComponent>().AbilityOwner = Owner;
            Abilities.Add(ability);

            if (!ability.IsInited)
                ability.Init();

            ability.Command(new ExecutePassiveAbilityCommand { Enabled = true, Owner = from, Target = to });
        }

        public void ActivateAvailableAbility(Entity entity)
        {
            if (entity.IsPaused)
                entity.UnPause();

            AddAbility(entity, true);
        }

        public void RemoveToAvailableAbility(Entity ability)
        {
            AvailableAbilities.Add(ability);
            Abilities.Remove(ability);

            IndexToAbility.Remove(ability.GetComponent<ActorContainerID>().ContainerIndex);
            GuidToAbility.Remove(ability.GUID);

            if (ability.TryGetComponent(out AdditionalAbilityIndexComponent component))
            {
                foreach (var i in component.AdditionalIndeces)
                    IndexToAbility.Remove(i);
            }

            ability.Pause();
        }

        public HECSPooledArray<Entity> GetAllAbilities()
        {
            var pool = HECSPooledArray<Entity>.GetArray(AvailableAbilities.Count + Abilities.Count);

            foreach (var a in AvailableAbilities)
                pool.Add(a);

            foreach (var a in Abilities)
                pool.Add(a);

            return pool;
        }

        public void RemoveAbility(Entity ability)
        {
            Abilities.Remove(ability);
            IndexToAbility.Remove(ability.GetComponent<ActorContainerID>().ContainerIndex);
            GuidToAbility.Remove(ability.GUID);

            if (ability.TryGetComponent(out AdditionalAbilityIndexComponent component))
            {
                foreach (var i in component.AdditionalIndeces)
                    IndexToAbility.Remove(i);
            }

            ability.Dispose();
        }

        public void Dispose()
        {
            foreach (var a in Abilities)
                a.Dispose();

            foreach (var ab in AvailableAbilities)
                ab.Dispose();

            Abilities.Clear();
            AvailableAbilities.Clear();
            IndexToAbility.Clear();
        }
    }
}