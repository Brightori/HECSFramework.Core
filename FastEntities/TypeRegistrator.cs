namespace HECSFramework.Core
{
    internal abstract class TypeRegistrator
    {
       public abstract void RegisterWorld(World world);
       public abstract void UnRegisterWorld(World world);
    }

    internal sealed class TypeRegistrator<T> : TypeRegistrator where T : struct, IFastComponent
    {
        public override void RegisterWorld(World world)
        {
            var collection = FastComponentProvider<T>.ComponentsToWorld;
            collection.AddToIndex(new FastComponentProvider<T>(world), world.Index);
        }

        public override void UnRegisterWorld(World world)
        {
            FastComponentProvider<T>.ComponentsToWorld.Data[world.Index].Dispose();
        }
    }
}