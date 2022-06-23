using HECSFramework.Core;
using System.Collections.Generic;

namespace Components
{
    [Documentation(Doc.Abilities, Doc.Predicates, "predicates for abilities")]
    public partial class PredicatesComponent : BaseComponent
    {
        public List<IPredicate> Predicates = new List<IPredicate>(4);

        /// <summary>
        /// сюда сначала передаём цель, потом владельца предиката, 
        /// но в некоторых кейсах нужно в цель передавать владельца,
        /// смотрите внимательно что делает данный предикат
        /// </summary>
        /// <param name="target">передаем сюда цель</param>
        /// <param name="owner">передаем сюда владельца предиката</param>
        /// <returns></returns>
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