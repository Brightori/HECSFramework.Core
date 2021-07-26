using HECSFramework.Core;
using System;

namespace Components
{
    [Serializable]
    [Documentation("Ability", "���� ��������� ���������� ��� ������, � ������ ������������� �������� ��� �� �������")]
    public class AbilityOwnerComponent : BaseComponent
    {
        public IEntity AbilityOwner;
    }
}