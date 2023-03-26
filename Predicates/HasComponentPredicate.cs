using System;
using HECSFramework.Core;

namespace Predicates
{
    [Serializable]
    [Documentation(Doc.Predicates, Doc.HECS, "this predicate check for available component on entity")]
    public sealed partial class HasComponentPredicate : IPredicate
    {
        private enum Contains { Contain, NotContain }
#if UNITY_2017_1_OR_NEWER
        [UnityEngine.SerializeField]
        [Sirenix.OdinInspector.PropertyOrder(2)]
#endif
        private Contains entityShouldContain = Contains.Contain;

#if UNITY_2017_1_OR_NEWER
        [Sirenix.OdinInspector.ReadOnly]
        [Sirenix.OdinInspector.PropertyOrder(-1)]
#endif
        public int Index;

        public bool IsReady(Entity target, Entity owner = null)
        {
            if (!TypesMap.GetComponentInfo(Index, out var info)) return true;

            if (target == null)
                return true;

            switch (entityShouldContain)
            {
                case Contains.Contain:
                    return target.ContainsMask(info.ComponentsMask.TypeHashCode);
                case Contains.NotContain:
                    return !target.ContainsMask(info.ComponentsMask.TypeHashCode);
            }

            return true;
        }
    }
}
