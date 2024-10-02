using System.Runtime.CompilerServices;
using Commands;
using Components;
using HECSFramework.Core;

namespace HECSFramework.Core
{
    public static class ComponentExtentions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetContainerIndex(this Entity entity) 
        {
            if (entity.TryGetComponent(out ActorContainerID actorContainerID))
                return actorContainerID.ContainerIndex;

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponentReactLocal<T>(this T component) where T: IComponent
        {
            component.Owner.Command(new AddComponentReactLocalCommand<T>(component));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponentReactGlobal<T>(this T component) where T : IComponent
        {
            component.Owner.World.Command(new AddComponentReactGlobalCommand<T>(component));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentReactLocal<T>(this T component) where T : IComponent
        {
            component.Owner.Command(new RemoveComponentReactLocalCommand<T>(component));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentReactGlobal<T>(this T component) where T : IComponent
        {
            component.Owner.World.Command(new RemoveComponentReactGlobalCommand<T>(component));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ComponentReactByTypeLocal<T>(this IComponent component) 
        {
            if (component is T needed)
                component.Owner.Command(new ComponentReactByTypeCommand<T>(needed));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ComponentReactByTypeGlobal<T>(this IComponent component)
        {
            if (component is T needed)
                component.Owner.World.Command(new ComponentReactByTypeGlobalCommand<T>(needed));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentReactByTypeLocal<T>(this IComponent component)
        {
            if (component is T needed)
                component.Owner.Command(new RemoveComponentReactByTypeCommand<T>(needed));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentReactByTypeGlobal<T>(this IComponent component)
        {
            if (component is T needed)
                component.Owner.World.Command(new RemoveComponentReactByTypeGlobalCommand<T>(needed));
        }
    }
}

namespace Commands
{
    public struct AddComponentReactLocalCommand<T> : ICommand where T: IComponent
    {
        public T Value;

        public AddComponentReactLocalCommand(T value)
        {
            Value = value;
        }
    }

    public struct ComponentReactByTypeCommand<T> : ICommand
    {
        public T Value;

        public ComponentReactByTypeCommand(T value)
        {
            Value = value;
        }
    }

    public struct ComponentReactByTypeGlobalCommand<T> : IGlobalCommand
    {
        public T Value;

        public ComponentReactByTypeGlobalCommand(T value)
        {
            Value = value;
        }
    }

    public struct RemoveComponentReactByTypeCommand<T> : ICommand
    {
        public T Value;

        public RemoveComponentReactByTypeCommand(T value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// this is generic command for listen components by interfaces or some abstract classes, if u operates by components type use RemoveComponentReactLocalCommand
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct RemoveComponentReactByTypeGlobalCommand<T> : IGlobalCommand
    {
        public T Value;

        public RemoveComponentReactByTypeGlobalCommand(T value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// this is generic command for listen components by interfaces or some abstract classes, if u operates by components type use RemoveComponentReactLocalCommand
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct RemoveComponentReactLocalCommand<T> : ICommand where T : IComponent
    {
        public T Value;

        public RemoveComponentReactLocalCommand(T value)
        {
            Value = value;
        }
    }

    public struct AddComponentReactGlobalCommand<T> : IGlobalCommand where T : IComponent
    {
        public T Value;

        public AddComponentReactGlobalCommand(T value)
        {
            Value = value;
        }
    }

    public struct RemoveComponentReactGlobalCommand<T> : IGlobalCommand where T : IComponent
    {
        public T Value;

        public RemoveComponentReactGlobalCommand(T value)
        {
            Value = value;
        }
    }
}
