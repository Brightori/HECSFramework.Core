using System;
using HECSFramework.Core;

namespace Helpers
{
    [Documentation(Doc.Attributes, "Provide DropDown list of specified identifiers")]
    public class EntityContainerDropDownAttribute : Attribute
    {
        public readonly string tagComponentName;

        public EntityContainerDropDownAttribute(string tagComponentName)
        {
            this.tagComponentName = tagComponentName;
        }
    }
}