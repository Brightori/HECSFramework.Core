using HECSFramework.Core;
using System.Collections.Generic;
using HECSFramework.Network;

namespace Components
{
    [Documentation("Ability", "тут мы храним предикаты для абилок")]
    public partial class PredicatesComponent : BaseComponent
    {
        [Field(0)]
        public List<IPredicate> predicates = new List<IPredicate>(4);

        public bool IsReady(IEntity entity)
        {
            foreach (var p in predicates)
            {
                if (p.IsReady(entity))
                    continue;
                else
                    return false;
            }

            return true;
        }
    }
}