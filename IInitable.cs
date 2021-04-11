namespace HECSFramework.Core 
{
    public interface IInitable
    {
        void Init();
    }

    public interface IAfterEntityInit
    {
        void AfterEntityInit();
    }

    public interface IInitable<T> where T : IEntity
    {
        void Init(T actor);
    }
}