namespace HECSFramework.Core
{
    public partial class GlobalUpdateSystem 
    {
        private UpdateModuleFixed fixedModule;
        private UpdateModuleLate lateModule;
        private UpdateModuleDefault defaultModule;
        private UpdateModuleGlobalStart startModule;
        private UpdateModuleAsync updateModuleAsync;

        public GlobalUpdateSystem()
        {
            fixedModule = new UpdateModuleFixed();
            lateModule = new UpdateModuleLate();
            defaultModule = new UpdateModuleDefault();
            startModule = new UpdateModuleGlobalStart();
            updateModuleAsync = new UpdateModuleAsync();
        }

        public void Register<T>(T registerUpdate, bool add) where T: IRegisterUpdatable
        {
            switch (registerUpdate)
            {
                case IUpdatable updatable:
                    defaultModule.Register(updatable, add);
                    break;
                case IFixedUpdatable fixedUpdatable:
                    fixedModule.Register(fixedUpdatable, add);
                    break;
                case ILateUpdatable lateUpdatable:
                    lateModule.Register(lateUpdatable, add);
                    break;
                case IAsyncUpdatable updatableAsync:
                    updateModuleAsync.Register(updatableAsync, add);
                    break;
                default:
                    UnityFuncs(registerUpdate, add);
                    AdditionalFuncs(registerUpdate, add);
                    break;
            }
        }

        partial void UnityFuncs(IRegisterUpdatable registerUpdatable, bool add);
        partial void AdditionalFuncs(IRegisterUpdatable registerUpdatable, bool add);

        public void Start()
            => startModule.GlobalStart();

        public void FixedUpdate()
            => fixedModule.FixedUpdateLocal();

        public void LateUpdate()
            => lateModule.UpdateLateLocal();

        public void Update()
        {
            defaultModule.UpdateLocal();
        }
    }
}

