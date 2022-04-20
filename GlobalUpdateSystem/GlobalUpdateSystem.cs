﻿using System;

namespace HECSFramework.Core
{
    public partial class GlobalUpdateSystem : IDisposable
    {
        private UpdateModuleFixed fixedModule;
        private UpdateModuleLate lateModule;
        private UpdateModuleDefault defaultModule;
        private UpdateModuleDeltaTime deltaUpdateModule;
        private UpdateModuleGlobalStart startModule;
        private PriorityUpdateModule priorityUpdateModule;

        public Action FinishUpdate { get; set; }

        public GlobalUpdateSystem()
        {
            fixedModule = new UpdateModuleFixed();
            lateModule = new UpdateModuleLate();
            defaultModule = new UpdateModuleDefault();
            deltaUpdateModule = new UpdateModuleDeltaTime();
            startModule = new UpdateModuleGlobalStart();
            priorityUpdateModule = new PriorityUpdateModule();
        }

        public void Register<T>(T registerUpdate, bool add) where T : IRegisterUpdatable
        {
            if (registerUpdate is IPriorityUpdatable priorityUpdatable)
                priorityUpdateModule.Register(priorityUpdatable, add);

            if (registerUpdate is INeedGlobalStart needGlobalStart)
                startModule.Register(needGlobalStart, add);

            if (registerUpdate is ILateStart needLateStart)
                startModule.Register(needLateStart, add);

            if (registerUpdate is IUpdatable updatable)
                defaultModule.Register(updatable, add);

            if (registerUpdate is IUpdatableDelta updatableDelta)
                deltaUpdateModule.Register(updatableDelta, add);

            if (registerUpdate is IFixedUpdatable fixedUpdatable)
                fixedModule.Register(fixedUpdatable, add);

            if (registerUpdate is ILateUpdatable lateUpdatable)
                lateModule.Register(lateUpdatable, add);

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
            priorityUpdateModule.UpdateLocal();
            defaultModule.UpdateLocal();
        }

        public void UpdateDelta(float deltaTime)
        {
            deltaUpdateModule.UpdateLocalDelta(deltaTime);
        }

        public void Dispose()
        {
            fixedModule.Dispose();
            lateModule.Dispose();
            defaultModule.Dispose();
            deltaUpdateModule.Dispose();
            startModule.Dispose();
            priorityUpdateModule.Dispose();
        }
    }
}