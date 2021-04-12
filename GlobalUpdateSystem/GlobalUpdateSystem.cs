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

        public void Register<T>(T registerUpdate, bool add) where T : IRegisterUpdatable
        {
            if (registerUpdate is INeedGlobalStart needGlobalStart)
                startModule.Register(needGlobalStart, true);

            if (registerUpdate is ILateStart needLateStart)
                startModule.Register(needLateStart, true);

            if (registerUpdate is IUpdatable updatable)
                defaultModule.Register(updatable, add);

            if (registerUpdate is IFixedUpdatable fixedUpdatable)
                fixedModule.Register(fixedUpdatable, add);

            if (registerUpdate is ILateUpdatable lateUpdatable)
                lateModule.Register(lateUpdatable, add);

            if (registerUpdate is IAsyncUpdatable updatableAsync)
                updateModuleAsync.Register(updatableAsync, add);

            UnityFuncs(registerUpdate, add);
            AdditionalFuncs(registerUpdate, add);
        }

        partial void UnityFuncs(IRegisterUpdatable registerUpdatable, bool add);
        partial void AdditionalFuncs(IRegisterUpdatable registerUpdatable, bool add);

        public void Start()
            => startModule.GlobalStart();

        public void LateStart()
            => startModule.LateStart();

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

