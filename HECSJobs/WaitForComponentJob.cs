using HECSFramework.Core;

[Documentation(Doc.GameLogic, Doc.HECS, Doc.Job, "this job wait until needed component appear at actor|entity")]
public readonly struct WaitFor<T> : IHecsJob where T : IComponent, new()
{
	public readonly Entity Entity;

    public WaitFor(Entity entity)
    {
        Entity = entity;
    }

    public void Run() { }

	public bool IsComplete()
	{
		return Entity.ContainsMask<T>();
	}
}

[Documentation(Doc.GameLogic, Doc.HECS, Doc.Job, "this job wait until needed component will be removed at actor|entity")]
public readonly struct WaitRemove<T> : IHecsJob where T : IComponent, new()
{
    public readonly Entity Entity;

    public WaitRemove(Entity entity)
    {
        Entity = entity;
    }

    public void Run() { }

    public bool IsComplete()
    {
        return !Entity.ContainsMask<T>();
    }
}