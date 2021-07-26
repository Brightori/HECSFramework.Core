using HECSFramework.Core;
using System;

namespace Components
{
    [Serializable]
    [Documentation("Ability", "Этот компонент обязателен для абилки, в оунера прокидывается персонаж что им владеет")]
    public class AbilityOwnerComponent : BaseComponent
    {
        public IEntity AbilityOwner;
    }
}