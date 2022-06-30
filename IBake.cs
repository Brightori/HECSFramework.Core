namespace HECSFramework.Core
{
    [Documentation(Doc.HECS, Doc.Baking, "This interface we using for transfering data to server for example, we run this interface on baking process, on realization of this interface we should gather data from enviroment")]
    public interface IBake
    {
        public void Bake(EntityContainer entityContainer);
    }

    [Documentation(Doc.HECS, Doc.Baking, "This interface we using for transfering data to server for example, we run this interface before main baking process, on realization of this interface we should gather data from enviroment")]
    public interface IPreBake
    {
        public void PreBake(EntityContainer entityContainer);
    }
}