using HECSFramework.Core;
using HECSFramework.Documentation;

namespace Components
{
    [Documentation(Doc.GameLogic, "Интерфейс которым помечаем компоненты и системы которые остаются жить после смерти ентити")]
    public interface IAfterLife
    {
    }
}