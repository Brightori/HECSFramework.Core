using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class DocumentationAttribute : Attribute
    {
        public List<string> SegmentType = new List<string>(4);
        public string Comment;

        public DocumentationAttribute(string segmentType, string comment = "")
        {
            SegmentType.Add(segmentType);
            Comment = comment;
        }

        /// <summary>
        /// last of tagsAndComment would be comment, all others going to tags
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="tagsAndComment"></param>
        public DocumentationAttribute(string tag, params string[] tagsAndComment)
        {
            SegmentType.Add(tag);

            if (tagsAndComment != null && tagsAndComment.Length > 0)
            {
                var index = Array.IndexOf(tagsAndComment, tagsAndComment.Last());
                for (int i = 0; i < tagsAndComment.Length; i++)
                {
                    if (i != index)
                    {
                        SegmentType.Add(tagsAndComment[i]);
                    }
                    else
                        Comment = tagsAndComment[i];
                }
            }
        }
    }

    public struct DocumentationRepresentation
    {
        public string[] SegmentTypes;
        public string[] Comments;
        public string DataType;
        public DocumentationType DocumentationType;
        public string FilePath;

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