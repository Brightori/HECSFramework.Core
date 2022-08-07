namespace HECSFramework.Core
{
    public partial interface ICounter
    {
        int Id { get; }
    }

    public partial interface ICounter<T> : ICounter
    {
        T Value { get; }
        void SetValue(T value);
        void ChangeValue(T value);
    }

    public interface ISubCounter : ICounter
    {
        public int SubId { get; }
    }

    public interface ICompositeCounter<T> : ICounter
    {
        T GetValue(int key);
        void SetValue(int key, T value);
        void ChangeValue(int key, T value);
    }

    public interface ICompositeGetValue<T> : ICounter
    {
        ICounter<T> Current { get; }
        ICompositeGetValue<T> Prev { get; }
        ICompositeGetValue<T> Next { get; }
    }

    public sealed class CompositeValueHolder<T> : ICompositeGetValue<T> where T : struct
    {
        public ICounter<T> Current { get; set;  }
        public ICompositeGetValue<T> Prev { get; set; }
        public ICompositeGetValue<T> Next { get; set; }
        public int Id => Current.Id;
    }

    public interface IBaseValue<T> : ICounter<T>
    {
        T GetBaseValue { get; }
    }

    public interface IMaxValue<T>
    {
        T MaxValue { get; }
    }

    public interface ICounterModifiable<T> : ICounter<T>, IMofifiersProcessor<T> where T : struct
    {
    }

    public interface ICompositeCounterModifiable<T> : ICompositeCounter<T>, IMofifiersCompositeProcessor<T> where T: struct
    {
        ICompositeGetValue<T> GetCompositeValue { get; }
        bool ContainsId(int id);
    }
}