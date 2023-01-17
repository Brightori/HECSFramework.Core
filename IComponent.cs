﻿namespace HECSFramework.Core
{
    public interface IComponent  : IHaveOwner
    {
        bool IsAlive { get; set; }
        int GetTypeHashCode { get; }
    }

    [Documentation(Doc.HECS, "Interface tag for singleton components at the world")]
    public interface IWorldSingleComponent : IComponent { }
}