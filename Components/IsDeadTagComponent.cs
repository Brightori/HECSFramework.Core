using System;
using HECSFramework.Core;

namespace Components
{
    /// <summary>
    /// if u dont need kill entity instantly u should add VisualinProgressTag and remove it when entity should be destroyed
    /// </summary>
    [Serializable][Documentation(Doc.Character, Doc.Tag, "We mark dead characters")]
    public sealed class IsDeadTagComponent : BaseComponent 
    {
        public int FrameToDeath = 3;

        /// <summary>
        /// default ticks to death
        /// </summary>
        public void Reset()
        {
            FrameToDeath = 3;
        }
    }
}