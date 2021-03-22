namespace HECSFramework.Core 
{

    public interface IRegisterUpdatable { }

    public interface IUpdatable : IRegisterUpdatable
    {
        void UpdateLocal();
    }
    
    public interface ILateUpdatable : IRegisterUpdatable
    {
        void UpdateLateLocal();
    }

    public interface IFixedUpdatable : IRegisterUpdatable
    {
        void FixedUpdateLocal();
    }


}