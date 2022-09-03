using HECSFramework.Core;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace Systems
{
    public sealed partial class PoolingSystem : BaseSystem, IReactComponent
    {
        private int maxCapacity = 512;

        private Dictionary<int, Stack<IComponent>> poolOfComponents = new Dictionary<int, Stack<IComponent>>(8);
        public Guid ListenerGuid { get; } = Guid.NewGuid();
        private ObjectPool<ArrayBufferWriter<byte>> arrayBufferWriters;

        public override void InitSystem()
        {
            arrayBufferWriters = new ObjectPool<ArrayBufferWriter<byte>>(CreateArrayBuffer);
        }

        private ArrayBufferWriter<byte> CreateArrayBuffer()
        {
            return new ArrayBufferWriter<byte>(2048);
        }

        public ArrayBufferWriter<byte> GetArrayBuffer()
        {
            return arrayBufferWriters.GetObject();
        }

        /// <summary>
        /// we clear array buffer when return to pool
        /// </summary>
        /// <param name="arrayBuffer"></param>
        public void ReleaseArrayBuffer(ArrayBufferWriter<byte> arrayBuffer)
        {
            arrayBuffer.Clear();
            arrayBufferWriters.PutObject(arrayBuffer);
        }

        public T GetComponentFromPool<T>(ref HECSMask mask) where T : IComponent
        {
            var index = mask.Index;

            if (poolOfComponents.ContainsKey(index))
            {
                if (poolOfComponents[index].Count > 0)
                {
                    var needed = (T)poolOfComponents[index].Pop();
                    needed.IsAlive = true;
                    needed.UnRegister();
                    return needed;
                }
            }
            else
                poolOfComponents.Add(index, new Stack<IComponent>());

            return (T)TypesMap.GetComponentFromFactory(mask.TypeHashCode);
        }

        public void ComponentReact<T>(T component, bool isAdded) where T : IComponent
        {
            if (!isAdded)
            {
                var index = component.ComponentsMask.Index;
                
                if (poolOfComponents.ContainsKey(index) && poolOfComponents[index].Count < maxCapacity)
                    poolOfComponents[index].Push(component);
            }
        }
    }
}