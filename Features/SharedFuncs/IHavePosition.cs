namespace HECSFramework.Core
{
    public interface IHavePosition 
    {
        //in monobehaviours u should make it false on destroy
        public bool IsAlive { get; }

#if UNITY_2017_1_OR_NEWER
        UnityEngine.Vector3 GetPosition { get; }
#else

        System.Numerics.Vector3 GetPosition { get; } 
#endif
    }
}

