using HECSFramework.Core;
using System;

namespace HECSFramework.Core
{
    public class WaitUntil : Awaiter
    {
        private Func<bool> predicate;

        public override bool IsCompleted => predicate.Invoke();

        public WaitUntil(Func<bool> predicate)
        {
            this.predicate = predicate;
        }
    }
}
