using HECSFramework.Core;
using HECSFramework.Documentation;
using System.Collections.Generic;
using System;

namespace Commands
{
    [Documentation(Doc.GameLogic, "Команда в которой мы задаём таймер, и колбэчим когда таймер истёк")]
    public struct WaitAndCallbackCommand : IGlobalCommand
    {
        public float Timer;
        public Action CallBack;
        public bool IsOnRun;

    }
}