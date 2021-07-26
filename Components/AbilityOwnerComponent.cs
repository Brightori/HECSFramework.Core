using HECSFramework.Core;
using System;

namespace Components
{
    [Serializable]
    [Documentation("Ability", "���� ��������� ���������� ��� ������, � ������ ������������� �������� ��� �� �������")]
    [CustomResolver]
    public class AbilityOwnerComponent : BaseComponent
    {
        public IEntity AbilityOwner;
    }
}