using HECSFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server.ECS.HECS.Core.Awaiters
{
    public sealed class WaitForSeconds : Awaiter
    {
        private Func<bool> predicate;

        public override bool IsCompleted => predicate.Invoke();

        public WaitForSeconds(float second)
        {
            long startTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
           
            this.predicate = () =>
            {
                float timer = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTimeStamp) / 1_000.0f;
                return timer >= second;
            };
        }
    }
}
