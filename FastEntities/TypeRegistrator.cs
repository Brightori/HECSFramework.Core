namespace HECSFramework.Core
{
    internal abstract class TypeRegistrator
    {
       public abstract void RegisterWorld(World world);
       public abstract void UnRegisterWorld(World world);
    }

    internal sealed class TypeRegistrator<T> : TypeRegistrator where T : struct, IData
    {
        public override void RegisterWorld(World world)
        {
            var collection = ComponentProvider<T>.ComponentsToWorld;
            collection.AddToIndex(new ComponentProvider<T>(world), world.Index);
        }

        public override void UnRegisterWorld(World world)
        {
            ComponentProvider<T>.ComponentsToWorld.Data[world.Index].Dispose();
            ComponentProvider<T>.ComponentsToWorld.Data[world.Index] = null;
        }
    }
}
