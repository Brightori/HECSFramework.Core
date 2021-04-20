using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class DocumentationAttribute : Attribute
    {
        public string SegmentType;
        public string Comment;

        public DocumentationAttribute(string segmentType, string comment = "")
        {
            SegmentType = segmentType;
            Comment = comment;
        }
    }

    public struct DocumentationRepresentation
    {
        public string[] SegmentTypes;
        public string[] Comments;
        public string DataType;
        public DocumentationType DocumentationType;

        public override bool Equals(object obj)
        {
            return obj is DocumentationRepresentation representation &&
                   EqualityComparer<string[]>.Default.Equals(SegmentTypes, representation.SegmentTypes) &&
                   EqualityComparer<string[]>.Default.Equals(Comments, representation.Comments) &&
                   DataType == representation.DataType &&
                   DocumentationType == representation.DocumentationType;
        }

        public override int GetHashCode()
        {
            int hashCode = 1939168364;
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(SegmentTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(Comments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DataType);
            hashCode = hashCode * -1521134295 + DocumentationType.GetHashCode();
            return hashCode;
        }
    }
}