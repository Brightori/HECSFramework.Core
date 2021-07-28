using HECSFramework.Core;
using System.Collections.Generic;

namespace Components
{
    [Documentation("Ability", "тут мы храним предикаты для абилок")]
    public partial class PredicatesComponent : BaseComponent
    {
        private List<IPredicate> predicates = new List<IPredicate>(4);

        public bool IsReady(IEntity entity)
        {
            foreach (var p in predicates)
            {
                if (!p.IsReady(entity))
                    return false;
            }

            return true;
        }
    }
}