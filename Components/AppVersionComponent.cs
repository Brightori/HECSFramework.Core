using HECSFramework.Core;
using System;

namespace Components
{
    [Serializable]
    [Documentation("GameLogic", "Компонент в котором мы храним версию")]
    public class AppVersionComponent : BaseComponent
    {
        [Field(0)]
        public int Version = 101;
    }
}