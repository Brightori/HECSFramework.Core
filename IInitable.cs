namespace HECSFramework.Core 
{
    public interface IInitable
    {
        void Init();
    }

    /// <summary>
    /// Компонент, помеченный этим интерфейсом не инитится, если энтити, его содержащая, была загружена из сериализованных данных
    /// </summary>
    public interface IIgnoreLoadInit
    {
    }

    public interface IAfterEntityInit
    {
        void AfterEntityInit();
    }

    public interface IInitable<T> 
    {
        void Init(T obj);
    }
}