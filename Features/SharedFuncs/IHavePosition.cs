namespace HECSFramework.Core
{
    public interface IHavePosition
    {
#if UNITY_2017_1_OR_NEWER
        UnityEngine.Vector3 GetPosition { get; }
#else

        System.Numerics.Vector3 GetPosition { get; } 
#endif
    }
}

