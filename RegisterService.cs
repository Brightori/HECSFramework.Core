namespace HECSFramework.Core
{
    public partial class RegisterService : IRegisterService
    {
        public void RegisterSystem(ISystem system)
        {
            if (system is IRegisterUpdatable asyncupdate)
                system.Owner.World.RegisterUpdatable(asyncupdate, true);

            if (system is IReactEntity enitiesChanges)
                system.Owner.World.AddEntityListener(enitiesChanges, true);

            if (system is IReactComponent componentsChanges)
                system.Owner.World.AddGlobalReactComponent(componentsChanges);

            RegisterAdditionalSystems(system);
            BindSystem(system);
        }

        public void UnRegisterSystem(ISystem system)
        {
            if (system is IRegisterUpdatable asyncupdate)
                system.Owner.World.RegisterUpdatable(asyncupdate, true);

            if (system is IReactEntity enitiesChanges)
                system.Owner.World.AddEntityListener(enitiesChanges, false);

            if (system is IReactComponent componentsChanges)
                system.Owner.World.RemoveGlobalReactComponent(componentsChanges);

            UnRegisterAdditionalSystems(system);
            UnBindSystem(system);
        }

        partial void BindSystem(ISystem system);
        
        private void UnBindSystem(ISystem system)
        {
            system.Owner.EntityCommandService.ReleaseListener(system);
            system.Owner.World.RemoveGlobalReactCommand(system);
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
        void RegisterSystem(ISystem system);
        void UnRegisterSystem(ISystem system);
    }
}