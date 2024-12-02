using System.Collections.Generic;

namespace HECSFramework.Core
{
    public partial class HECSDocumentation
    {
        public List<DocumentationRepresentation> Documentations = new List<DocumentationRepresentation>();
    }

    public enum DocumentationType { Common, Component, System }
}