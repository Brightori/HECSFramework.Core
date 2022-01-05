using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HECSFramework.Core.Generator
{
    public partial class CodeGenerator
    {
        public string GetDocumentation()
        {
            var tree = new TreeSyntaxNode();

            tree.Add(new UsingSyntax("System.Collections.Generic", 1));
            tree.Add(new NameSpaceSyntax("HECSFramework.Core"));
            tree.Add(new LeftScopeSyntax());
            tree.Add(new TabSimpleSyntax(1, "public partial class HECSDocumentation"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(GetDocumentationConstructor());
            tree.Add(new RightScopeSyntax(1));
            tree.Add(new RightScopeSyntax());

            return tree.ToString();
        }

        private ISyntax GetDocumentationConstructor()
        {
            var tree = new TreeSyntaxNode();
            tree.Add(new TabSimpleSyntax(2, "public HECSDocumentation()"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new TabSimpleSyntax(3, "Documentations = new List<DocumentationRepresentation>"));
            tree.Add(new LeftScopeSyntax(3));
            tree.Add(GetDocumentRepresentationArray());
            tree.Add(new RightScopeSyntax(3,true));
            tree.Add(new RightScopeSyntax(2));
            return tree;
        }

        private ISyntax GetDocumentRepresentationArray()
        {
            var tree = new TreeSyntaxNode();

            var typeHolder = new Dictionary<Type, (List<string> segments, List<string> comments, string Type)>(64);

            foreach (var t in Assembly)
            {
                if (!t.CustomAttributes.Any(x => x.AttributeType == typeof(DocumentationAttribute)))
                    continue;

                var attrs = t.GetCustomAttributes().ToArray();
                
                if (attrs != null && attrs.Length > 0)
                {
                    typeHolder.Add(t, (new List<string>(), new List<string>(), t.Name));

                    foreach (var a in attrs)
                    {
                        if (a is DocumentationAttribute documentation)
                        {
                            foreach (var d in documentation.SegmentType)
                                typeHolder[t].segments.Add(d);

                            typeHolder[t].comments.Add(documentation.Comment);
                        }
                    }
                }
            }

            foreach (var collected in typeHolder)
            {
                tree.Add(new TabSimpleSyntax(4, "new DocumentationRepresentation"));
                tree.Add(new LeftScopeSyntax(4));
                tree.Add(GetStringArray("SegmentTypes", collected.Value.segments));
                tree.Add(GetStringArray("Comments", collected.Value.comments));
                tree.Add(new TabSimpleSyntax(5, $"DataType = {CParse.Quote + collected.Value.Type + CParse.Quote},"));
                tree.Add(GetDocumentationType(collected.Key));
                tree.Add(new RightScopeSyntax(4){ IsCommaNeeded = true });
            }
            return tree;
        }

        private ISyntax GetDocumentationType(Type type)
        {
            var tree = new TreeSyntaxNode();
            string documentationType;

            if (componentTypes.Contains(type))
                documentationType = "DocumentationType.Component";
            else if (systems.Contains(type))
                documentationType = "DocumentationType.System";
            else
                documentationType = "DocumentationType.Common";

            tree.Add(new TabSimpleSyntax(5, $"DocumentationType = {documentationType},"));

            return tree;
        }

        private ISyntax GetStringArray(string name, List<string> toArray)
        {
            var tree = new TreeSyntaxNode();
            var body = new TreeSyntaxNode();

            tree.Add(new TabSimpleSyntax(5, $"{name} = new string[]"));
            tree.Add(new LeftScopeSyntax(5));
            tree.Add(body);
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(5), new SimpleSyntax(CParse.RightScope+CParse.Comma+CParse.Paragraph)));

            foreach (var s in toArray)
            {
                if (string.IsNullOrEmpty(s))
                    continue;

                body.Add(new TabSimpleSyntax(6, $"{CParse.Quote + s + CParse.Quote + CParse.Comma}"));
            }

            return tree;
        }
    }
}