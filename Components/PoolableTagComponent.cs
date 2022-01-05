using HECSFramework.Core;
using System;

namespace Components
{
    [Serializable]
    [Documentation(Doc.GameLogic, "Компонент который отмечает ентити которые надо пулить, по дефолту используюется в Unity части")]
    public partial class PoolableTagComponent : BaseComponent, IAfterLife
    {
    }
}