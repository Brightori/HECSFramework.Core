using HECSFramework.Core;
using System;

namespace Components
{
    [Serializable]
    [Documentation("GameLogic", "��������� � ������� �� ������ ������")]
    public partial class AppVersionComponent : BaseComponent
    {
        [Field(0)]
        public int Version = 101;
    }
}