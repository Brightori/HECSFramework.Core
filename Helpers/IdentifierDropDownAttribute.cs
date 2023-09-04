using System;
using HECSFramework.Core;

namespace Helpers
{
    [Documentation(Doc.Attributes, "Provide DropDown list of specified identifiers")]
    public class IdentifierDropDownAttribute : Attribute
    {
        public readonly string identifierType;

        public IdentifierDropDownAttribute(string identifierType)
        {
            this.identifierType = identifierType;
        }
    }
}