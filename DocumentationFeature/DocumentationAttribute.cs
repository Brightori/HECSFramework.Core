using System;

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
    }
}