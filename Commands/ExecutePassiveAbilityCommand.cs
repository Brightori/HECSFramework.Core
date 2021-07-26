using HECSFramework.Core;

namespace Commands
{
    [Documentation("Ability", "Команда которой активируем пассивные абилки, её надо отправлять абилковладельцу")]
    public struct ExecutePassiveAbilityCommand : ICommand
    {
        public IEntity Target;
        public IEntity Owner;
        public bool Enabled;
    }
}