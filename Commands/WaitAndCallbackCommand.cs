using HECSFramework.Core;
using HECSFramework.Documentation;
using System.Collections.Generic;
using System;
using Strategies;

namespace Commands
{
    [Documentation(Doc.GameLogic, "Команда в которой мы задаём таймер, и колбэчим когда таймер истёк")]
    public struct WaitAndCallbackCommand : IGlobalCommand
    {
        public float Timer;
        public Action<IEntity> CallBack;
        public bool IsOnRun;
        public IEntity CallBackWaiter;
    }
}