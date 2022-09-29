using System;
using System.Runtime.InteropServices;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Animation, "this component holds animations scenarios with timings ")]
    public sealed partial class ScenarioAnimationComponent : BaseComponent
    {
        public ScenarioAnimation[] ScenarioAnimations;

        public bool TryGetScenarioLenght(int scenarioIndex, out float result)
        {
            foreach(var s in ScenarioAnimations)
            {
                if (s.ScenarioIndex == scenarioIndex)
                {
                    result = s.GetScenarioLenght();
                    return true;
                }
            }

            result = 0;
            return false;
        }
    }

    [Serializable]
    public partial struct ScenarioAnimation
    {
#if UNITY_EDITOR
        [UnityEngine.HideInInspector]
#endif
        public int ScenarioIndex;
      
        public AnimationHECSInfo[] AnimationSteps;

        public float GetScenarioLenght()
        {
            float lenght = 0;

            if (AnimationSteps != null)
            {
                foreach (var animationStep in AnimationSteps)
                {
                    lenght += animationStep.AnimationLenght;
                }
            }

            return lenght;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public partial struct AnimationHECSInfo
    {
#if UNITY_EDITOR
        [UnityEngine.HideInInspector]
#endif
        public int AnimationEvent;

#if UNITY_EDITOR
        [Sirenix.OdinInspector.ReadOnly]
#endif
        public float AnimationLenght;
    }
}