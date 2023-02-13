using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public sealed partial class SystemRegisterService : IRegisterService
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterSystem<T>(T system) where T : ISystem
        {
            if (system is IRegisterUpdatable asyncupdate)
                system.Owner.World.RegisterUpdatable(asyncupdate, true);

            if (system is IReactEntity enitiesChanges)
                system.Owner.World.AddEntityListener(enitiesChanges, true);

            RegisterAdditionalSystems(system);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnRegisterSystem<T>(T system) where T: ISystem
        {
            if (system is IRegisterUpdatable asyncupdate)
                system.Owner.World.RegisterUpdatable(asyncupdate, false);

            if (system is IReactEntity enitiesChanges)
                system.Owner.World.AddEntityListener(enitiesChanges, false);

            UnRegisterAdditionalSystems(system);
            system.Owner.World.AdditionalProcessing(system, system.Owner, false);

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