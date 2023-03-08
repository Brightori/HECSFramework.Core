using System;

namespace HECSFramework.Core
{
    public abstract class UniversalReact : IDisposable
    {
        public abstract void Dispose();
        public abstract void React(IComponent component, bool added);
    }
}
