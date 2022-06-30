using System;
using HECSFramework.Core;

namespace Systems
{
    public sealed class RegisterSingleWorldComponentsSystem : BaseSystem, IReactComponent
    {
        private IAddSingleComponent addSingleComponent;

        public Guid ListenerGuid { get; }

        public void ComponentReact<T>(T component, bool isAdded) where T : IComponent
        {
            if (component is IWorldSingleComponent)
                addSingleComponent.AddSingleWorldComponent(component, isAdded);
        }

        public override void InitSystem()
        {
            addSingleComponent = Owner.World;

            foreach (var e in Owner.World.Entities)
            {
                foreach (var c in e.GetComponentsByType<IWorldSingleComponent>())
                    addSingleComponent.AddSingleWorldComponent(c, true);
            }
        }
    }
}