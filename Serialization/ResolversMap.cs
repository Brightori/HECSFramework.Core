using System;
using System.Collections.Generic;

public partial interface IData { }

namespace HECSFramework.Core
{
    public delegate void ProcessResolverContainer(ref ResolverDataContainer dataContainerForResolving, ref IEntity entity);
    public delegate ResolverDataContainer GetContainer<T>(T component) where T : IComponent;

    public partial class ResolversMap
    {
        private Dictionary<int, IResolverProvider> resolvers;

        private GetContainer<IComponent> GetComponentContainerFunc;
        
        /// <summary>
        /// Factory resolver data containers to components
        /// </summary>
        public Func<ResolverDataContainer, IComponent> GetComponentFromContainer { get; private set; }
        
        /// <summary>
        /// Its for resolving container when we alrdy have entity
        /// </summary>
        public ProcessResolverContainer ProcessResolverContainer { get; private set; }

        public void LoadDataFromContainer(ResolverDataContainer dataContainerForResolving, int worldIndex = 0) => LoadDataFromContainerSwitch(dataContainerForResolving, worldIndex);

        public ResolverDataContainer GetComponentContainer<T>(T component) where T : IComponent => GetComponentContainerFunc(component);

        partial void LoadDataFromContainerSwitch(ResolverDataContainer dataContainerForResolving, int worldIndex);

        private ResolverDataContainer PackComponentToContainer(IComponent component, IData data)
        {
            return new ResolverDataContainer
            {
                Data = data,
                EntityGuid = component.Owner.GUID,
                Type = 0,
                TypeHashCode = component.GetTypeHashCode,
            };
        }
    }

    public struct ResolverDataContainer : IData
    {
        /// <summary>
        /// 0 - Component, 1  - System, 2- Command
        /// </summary>
        public int Type;
        public int TypeHashCode;
        public IData Data;
        public Guid EntityGuid;
        public bool IsSyncSelf;
    }

    public interface IResolverProvider
    {
        ResolverDataContainer GetDataContainer<T>(T data);
        void ResolveData(ResolverDataContainer data, ref IEntity entity);
    }

    public interface IResolver<T>
    {
        void Out(ref T data);
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        public readonly int Queue;

        public FieldAttribute(int queue)
        {
            Queue = queue;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class GenerateResolverAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class CustomResolverAttribute : Attribute
    {
    }
}