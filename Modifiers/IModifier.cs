namespace HECSFramework.Core
{
    public interface IModifier 
    {
        ModifierCalculationType GetCalculationType { get; }
    }

    public interface IModifier<T> : IModifier where T : struct
    {
        T GetValue { get; }

        void Modify(ref T currentMod);
    }

    public enum ModifierCalculationType
    {
        Add = 0,
        Subtract = 1,
        Multiply = 2,
        Divide = 3,
    }
}