using HECSFramework.Core;
#pragma warning disable
namespace Commands
{
    [Documentation("Ability", "Команда которой активируем, её надо отправлять абилковладельцу, пассивные и активные абилки различаются теперь тегами")]
    public partial struct ExecuteAbilityCommand : ICommand
    {
        public IEntity Target;
        public IEntity Owner;
        public bool Enabled;
    }
}