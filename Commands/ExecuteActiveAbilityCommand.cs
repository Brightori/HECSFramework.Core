using HECSFramework.Core;

namespace Commands
{
    [Documentation("Ability", "Команда которой активируем активные абилки, её надо отправлять абилковладельцу")]
    public struct ExecuteActiveAbilityCommand : ICommand
    {
        public IEntity Target;
        public IEntity Owner;
        public bool Enabled;
    }
}