using HECSFramework.Core;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace Systems
{
    public sealed partial class PoolingSystem : BaseSystem
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

        public void UpdateMaxCapacity(int maxCapacity)
        {
            this.maxCapacity = maxCapacity;
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
    }
}