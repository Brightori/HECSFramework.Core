using HECSFramework.Core;
using System.Collections.Generic;

namespace Components
{
    [Documentation("Ability", "тут мы храним предикаты для абилок")]
    public partial class PredicatesComponent : BaseComponent
    {
        public List<IPredicate> Predicates = new List<IPredicate>(4);

        public bool IsReady(IEntity target, IEntity owner = null)
        {
            if (Predicates.Count == 0) return true;

            foreach (var p in Predicates)
            {
                if (p.IsReady(target, owner))
                    continue;
                return false;
            }

            return true;
        }
    }
}