using System;
using HECSFramework.Core;
using Helpers;

namespace Components
{
    [Serializable][Documentation(Doc.HECS, Doc.Counters, "here we hold some counter value for compare him to our purpose")]
    public sealed class CounterInfoComponent : BaseComponent
    {
        [IdentifierDropDown("CounterIdentifierContainer")]
        public int Counter;
    }
}