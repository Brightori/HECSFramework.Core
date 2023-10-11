using System;
using System.Buffers;

namespace Helpers
{
    public ref struct HECSPooledArray<T>
    {
        public T[] Items;
        private int count;
        public int Count => count;

        public static HECSPooledArray<T> GetArray(int count = 64)
        {
            var hecsPool = new HECSPooledArray<T>(); 
            int lenght = 64;

            if (count > 64)
            {
                lenght = hecsPool.CalculateLenght(ref lenght, in count);
            }

            hecsPool.Items = ArrayPool<T>.Shared.Rent(lenght);
            hecsPool.count = 0;
            return hecsPool;
        }

        private int CalculateLenght(ref int currentCount, in int desiredCount)
        {
            currentCount *= 2;

            if (currentCount < desiredCount)
                CalculateLenght(ref currentCount, in desiredCount);

            return currentCount;
        }

        public void Release()
        {
            ArrayPool<T>.Shared.Return(Items, true);
        }

        public void Add(T element)
        {
            if (count + 1 > Items.Length)
                throw new OverflowException("pooled array not so big");

            Items[count] = element;
            count++;
        }

        public static HECSPooledArray<T> GetCopy(HECSPooledArray<T> copyFrom)
        {
            var hecspool = GetArray(copyFrom.Count);

            var currentCount = copyFrom.Count;
            for (var i = 0; i < currentCount; i++)
                hecspool.Add(copyFrom.Items[i]);

            return hecspool;
        }

        public static HECSPooledArray<T> GetCopy(T[] copyFrom)
        {
            var hecspool = GetArray(copyFrom.Length);
            var currentCount = copyFrom.Length;
            
            for (var i = 0; i < currentCount; i++)
                hecspool.Add(copyFrom[i]);

            return hecspool;
        }

        public void Dispose()
        {
            Release();
        }
    }
}