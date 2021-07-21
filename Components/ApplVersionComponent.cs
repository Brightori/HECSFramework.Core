using HECSFramework.Core;
using System;

namespace Components
{
    [Serializable]
    [Documentation("GameLogic", "��������� � ������� �� ������ ������")]
    public class ApplVersionComponent : BaseComponent
    {
        [Field(0)]
        public string Prefix = "a";
        [Field(1)]
        public int Version = 101;
        [Field(2)]
        public string Suffix = "a";
    }
}