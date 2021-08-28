using HECSFramework.Core;
using System.Collections.Generic;

namespace Components
{
    [Documentation("Ability", "тут мы храним предикаты для абилок")]
    public partial class PredicatesComponent : BaseComponent
    {
        public List<IPredicate> Predicates = new List<IPredicate>(4);

        public bool IsReady(IEntity entity)
        {
            foreach (var p in Predicates)
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