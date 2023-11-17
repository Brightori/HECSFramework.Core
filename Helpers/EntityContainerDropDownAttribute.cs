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

    [Documentation(Doc.Attributes, "Provide DropDown list of specified identifiers")]
    public class EntityContainerIDDropDownAttribute : Attribute
    {
        public readonly string tagComponentName;

        public EntityContainerIDDropDownAttribute(string tagComponentName)
        {
            this.tagComponentName = tagComponentName;
        }
    }
}