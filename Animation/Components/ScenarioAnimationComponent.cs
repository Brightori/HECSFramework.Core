using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Animation, "this component holds animations scenarios with timings ")]
    public sealed partial class ScenarioAnimationComponent : BaseComponent
    {
        public ScenarioAnimation[] ScenarioAnimations;
    }

    [Serializable]
    public struct ScenarioAnimation
    {
        public int ScenarioIndex;
        public AnimationInfo[] AnimationSteps;

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
    public partial struct AnimationInfo
    {
        public int AnimationState;
        public int AnimationEvent;
        public float AnimationLenght;
    }
}