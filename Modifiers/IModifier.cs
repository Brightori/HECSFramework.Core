namespace HECSFramework.Core
{
    public interface IModifier 
    {
        ModifierCalculationType GetCalculationType { get; }
    }

    public interface IModifier<T> : IModifier
    {
        T Modify(ref T value);
    }

    public enum ModifierCalculationType
    {
        Add = 0,
        Subtract = 1,
        Multiply = 2,
        Divide = 3,
    }
}