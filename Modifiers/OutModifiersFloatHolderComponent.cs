using System;
using System.Collections.Generic;
using HECSFramework.Core;
using HECSFramework.Core.Helpers;

namespace Components
{
    public abstract class OutModifiersFloatHolderComponent : BaseComponent, IOutCompositeModifier<float> 
    {
        protected Dictionary<int, OutModifiersContainer<IModifier<float>, float>> outModifiers = 
            new Dictionary<int, OutModifiersContainer<IModifier<float>, float>>();

        public void AddModifier(int key, Guid owner, IModifier<float> modifier)
        {
            outModifiers.AddOrGet(key, out var container);
            container.AddModifier(owner, modifier);
        }

        public void AddUniqueModifier(int key, Guid owner, IModifier<float> modifier)
        {
            outModifiers.AddOrGet(key, out var container);
            container.AddUniqueModifier(owner, modifier);
        }

        public void Calculate(int key, ref float value)
        {
            if (outModifiers.TryGetValue(key, out var container))
            {
                container.GetCalculatedValue(ref value);
            }
        }

        public void RemoveModifier(int key, Guid owner, IModifier<float> modifier)
        {
            if (outModifiers.TryGetValue(key, out var container))
            {
                container.RemoveModifier(owner, modifier);
            }
        }

        public void Reset()
        {
            foreach (var c in outModifiers)
            {
                c.Value.Reset();
            }
        }
    }
}