using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HECSFramework.Core
{
 
    public interface IAwaiter
    {
        bool IsCompleted { get; }

        bool TryFinalize();
    }
}
