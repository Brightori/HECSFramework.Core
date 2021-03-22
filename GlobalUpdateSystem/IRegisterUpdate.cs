namespace HECSFramework.Core
{
    public interface IRegisterUpdate<T> where T: IRegisterUpdatable
    {
        void Register(T updatable, bool add);
    }
}
