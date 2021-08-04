using HECSFramework.Core;
using System;

namespace Components
{
    [Serializable]
    [Documentation("GameLogic", "Компонент в котором мы храним версию")]
    public partial class AppVersionComponent : BaseComponent
    {
        public int version = 101;
    }
}