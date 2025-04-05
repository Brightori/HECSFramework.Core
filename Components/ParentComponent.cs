using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.HECS, "on this component we can build a hierarchy and refer to the ancestor for data manipulation/query")]
    public sealed class ParentComponent : BaseComponent
    {
        public AliveEntity Parent;
    }
}