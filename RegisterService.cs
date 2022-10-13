namespace HECSFramework.Core
{
    public sealed partial class RegisterService : IRegisterService
    {
        public void RegisterSystem<T>(T system) where T : ISystem
        {
            if (system is IRegisterUpdatable asyncupdate)
                system.Owner.World.RegisterUpdatable(asyncupdate, true);

            if (system is IReactEntity enitiesChanges)
                system.Owner.World.AddEntityListener(enitiesChanges, true);

            if (system is IReactComponent componentsChanges)
                system.Owner.World.AddGlobalReactComponent(componentsChanges);

            if (system is IReactComponentLocal reactComponent)
                system.Owner.RegisterComponentListenersService.AddLocalListener(system, reactComponent);

            RegisterAdditionalSystems(system);
        }

        public void UnRegisterSystem<T>(T system) where T: ISystem
        {
            if (system is IRegisterUpdatable asyncupdate)
                system.Owner.World.RegisterUpdatable(asyncupdate, false);

            if (system is IReactEntity enitiesChanges)
                system.Owner.World.AddEntityListener(enitiesChanges, false);

            if (system is IReactComponent componentsChanges)
                system.Owner.World.RemoveGlobalReactComponent(componentsChanges);

            if (system is IReactComponentLocal)
                system.Owner.RegisterComponentListenersService.ReleaseListener(system);

            UnRegisterAdditionalSystems(system);
            TypesMap.UnBindSystem(system);
        }
        
        //for different custom systems on unity or server side
        partial void RegisterAdditionalSystems(ISystem system);
        partial void UnRegisterAdditionalSystems(ISystem system);
    }
}

namespace HECSFramework.Core
{
    public interface IRegisterService
    {
        void RegisterSystem<T>(T system) where T: ISystem;
        void UnRegisterSystem<T>(T system) where T : ISystem;
    }
}