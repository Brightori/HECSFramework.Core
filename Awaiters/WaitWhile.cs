using System;

namespace HECSFramework.Core
{
    public class WaitWhile : Awaiter
    {
        private Func<bool> predicate;

        public override bool IsCompleted => !predicate.Invoke();

        public WaitWhile(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

      
    }
}
