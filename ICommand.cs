namespace HECSFramework.Core
{
    public interface ICommand
    {
    }

    /// <summary>
    /// это тег для команд которые подразумеваются для глобального использования
    /// </summary>
    public interface IGlobalCommand : ICommand { }

    public interface IReactCommand<T> where T : struct, ICommand
    {
        void CommandReact(T command);
    }

    public interface IReactGlobalCommand<T> where T : struct, ICommand, IGlobalCommand
    {
        void CommandGlobalReact(T command);
    }
}