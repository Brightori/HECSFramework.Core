namespace HECSFramework.Core
{
    public partial interface IPredicate
    {
        /// <summary>
        /// тут мы сначала передаем цель, так как у нас изначально были предикаты только на цель
        /// второй аргумент не обязательный, но в целом у нас есть предикаты которые требуют наличия
        /// владельца и цели, например сверка фракций для френдли файр
        /// </summary>
        /// <param name="target">здесь нужно указывать кого мы будем проверять в первую очередь</param>
        /// <param name="owner">здесь мы передаем владельца предиката</param>
        /// <returns></returns>
        bool IsReady(IEntity target, IEntity owner = null);
    }

    public interface IPredicateContainer
    {
        IPredicate GetPredicate { get; }
    }
}