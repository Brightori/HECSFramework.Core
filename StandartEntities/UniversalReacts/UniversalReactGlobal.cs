using System;

namespace HECSFramework.Core
{
    public abstract class UniversalReactGlobal : IDisposable
    {
        public abstract void Dispose();
        public abstract void React(IComponent component, bool added);
        public abstract void ForceReact();
    }
}
