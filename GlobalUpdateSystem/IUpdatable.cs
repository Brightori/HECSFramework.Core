namespace HECSFramework.Core 
{

    public interface IRegisterUpdatable { }

    public interface IUpdatable : IRegisterUpdatable
    {
        void UpdateLocal();
    }

    public interface IUpdatableDelta : IRegisterUpdatable
    {
        void UpdateLocalDelta(float deltaTime);
    }

    public interface ILateUpdatable : IRegisterUpdatable
    {
        void UpdateLateLocal();
    }

    public interface IFixedUpdatable : IRegisterUpdatable
    {
        void FixedUpdateLocal();
    }

    public interface IPriorityUpdatable : IRegisterUpdatable
    {
        int Priority { get; }
        void PriorityUpdateLocal();
    }
}