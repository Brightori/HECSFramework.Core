using HECSFramework.Core;
using System;
using System.Collections.Generic;

namespace Commands
{
    [Documentation(Doc.GameLogic, "The command in which we set the timer, and callback with Character when the timer expires")]
    public struct WaitAndEntityCallbackCommand : IGlobalCommand
    {
        public float Timer;
        public Action<IEntity> CallBack;
        public bool IsOnRun;
        public IEntity CallBackWaiter;

        public override bool Equals(object obj)
        {
            return obj is WaitAndEntityCallbackCommand command &&
                   EqualityComparer<Action<IEntity>>.Default.Equals(CallBack, command.CallBack) &&
                   EqualityComparer<IEntity>.Default.Equals(CallBackWaiter, command.CallBackWaiter);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CallBack, CallBackWaiter);
        }
    }


    /// <summary>
    /// u should use constructor of this command
    /// </summary>
    [Documentation(Doc.GameLogic, "The command in which we set the timer, and call when the timer expires")]
    public struct WaitAndCallbackCommand : IGlobalCommand
    {
        public float Timer;
        public Action CallBack;
        public Guid Commandguid;

        public WaitAndCallbackCommand(float timer, Action callBack)
        {
            Timer = timer;
            CallBack = callBack;
            Commandguid = Guid.NewGuid();
        }

        public override bool Equals(object obj)
        {
            return obj is WaitAndCallbackCommand command &&
                   Commandguid.Equals(command.Commandguid);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Commandguid);
        }
    }
}