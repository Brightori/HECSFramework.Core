using System;
using HECSFramework.Core;

namespace Helpers
{
    [Documentation(Doc.Attributes, "Provide DropDown list of specified identifiers")]
    public class IdentifierDropDownAttribute : Attribute
    {
        public readonly string identifierType;
        public readonly string identifierType2;

        public IdentifierDropDownAttribute(string identifierType) : this(identifierType, "")
        {
        }

        public IdentifierDropDownAttribute(string identifierType, string identifierType2)
        {
            this.identifierType = identifierType;
            this.identifierType2 = identifierType2;
        }
    }
}