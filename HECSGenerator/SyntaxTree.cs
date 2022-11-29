using System.Collections.Generic;
using System.Linq;
using HECSFramework.Core.Helpers;

namespace HECSFramework.Core.Generator
{
    public interface ISyntax
    {
        string StringValue { get; set; }
        List<ISyntax> Tree { get; set; }
    }

    public class EmptySpaceSyntax : ISyntax
    {
        public List<ISyntax> Tree { get; set; } = new List<ISyntax>();
        public string StringValue { get; set; } = CParse.Space;
    }

    public class TreeSyntaxNode : ISyntax, IRawSyntaxData
    {
        public List<ISyntax> Tree { get; set; } = new List<ISyntax>();
        public string StringValue { get; set; } = "MainTree";
        public string[] RawData { get; set; }

        public void Add(ISyntax syntax)
        {
            Tree.Add(syntax);
        }

        public override string ToString()
        {
            var t = string.Empty;

            for (int i = 0; i < Tree.Count; i++)
            {
                t += Tree[i].ToString();
            }

            return t;
        }
    }

    public interface IRawSyntaxData
    {
        string[] RawData { get; set; }
    }

    public class ParagraphSyntax : SyntaxNode
    {
        public ParagraphSyntax()
        {
            StringValue = CParse.Paragraph;
        }

        public override string ToString()
        {
            return StringValue;
        }
    }

    public class SyntaxNode : ISyntax
    {
        public virtual string StringValue { get; set; }
        public List<ISyntax> Tree { get; set; } = new List<ISyntax>(8);
    }

    public class ReturnSyntax : ISyntax
    {
        public string StringValue { get; set; }
        public List<ISyntax> Tree { get; set; }

        public override string ToString()
        {
            return CParse.Return + CParse.Semicolon + CParse.Paragraph;
        }
    }

    public class SimpleSyntax : SyntaxNode
    {
        public string Data;

        public SimpleSyntax() { }

        public SimpleSyntax(string data)
        {
            Data = data;
        }

        public override string ToString()
        {
            return Data;
        }
    }

    public class TabSpaceSyntax : ISyntax
    {
        public string StringValue { get; set; }
        public List<ISyntax> Tree { get; set; }

        public int TabCount;

        public TabSpaceSyntax()
        {
            TabCount = 1;
        }

        public TabSpaceSyntax(int tabCount)
        {
            TabCount = tabCount;
        }

        public override string ToString()
        {
            StringValue = string.Empty;

            for (int i = 0; i < TabCount; i++)
            {
                StringValue += CParse.Tab;
            }

            return StringValue;
        }
    }

    public class FieldMember : SyntaxNode, IRawSyntaxData
    {
        public FieldMember()
        {
        }

        public FieldMember(string type)
        {
            StringValue = type;
        }

        public string[] RawData { get; set; }
    }

    public class TypeSyntax : ISyntax
    {
        public string StringValue { get; set; }
        public List<ISyntax> Tree { get; set; }
    }

    public class NameSpaceSyntax : SyntaxNode
    {
        public string Name;

        public NameSpaceSyntax()
        {
        }

        public NameSpaceSyntax(string name) : this()
        {
            Name = name;
        }

        public override string StringValue { get; set; } = CParse.Namespace;

        public override string ToString()
        {
            return StringValue + CParse.Space + Name + CParse.Paragraph;
        }
    }

    public class UsingSyntax : SyntaxNode
    {
        public string Name;
        private int paragraphCount = 1;

        public UsingSyntax(string name)
        {
            Name = name;
        }

        public UsingSyntax(string name, int paragraphAfter) : this(name)
        {
            paragraphCount += paragraphAfter;
        }

        public override string StringValue { get; set; } = CParse.Using;

        public override string ToString()
        {
            var s = string.Empty;
            s = StringValue + CParse.Space + Name + CParse.Semicolon;

            for (int i = 0; i < paragraphCount; i++)
            {
                s += CParse.Paragraph;
            }

            return s;
        }
    }

    public class LeftScopeSyntax : ISyntax
    {
        public bool IsWithoutParagraph;

        public LeftScopeSyntax()
        {
        }

        public LeftScopeSyntax(int tabSpace)
        {
            StringValue = string.Empty;

            for (int i = 0; i < tabSpace; i++)
            {
                StringValue += CParse.Tab;
            }

            StringValue += CParse.LeftScope;
        }

        public LeftScopeSyntax(int tabspace, bool isWithoutParagraph) : this(tabspace)
        {
            IsWithoutParagraph = isWithoutParagraph;
        }

        public List<ISyntax> Tree { get; set; } = new List<ISyntax>();
        public string StringValue { get; set; } = CParse.LeftScope;

        public override string ToString()
        {
            return StringValue + (IsWithoutParagraph ? "" : CParse.Paragraph);
        }
    }

    public class RightScopeSyntax : ISyntax
    {
        public List<ISyntax> Tree { get; set; } = new List<ISyntax>();
        public string StringValue { get; set; } = CParse.RightScope;
        public bool IsCommaNeeded;

        public RightScopeSyntax(int tabSpace)
        {
            StringValue = string.Empty;

            for (int i = 0; i < tabSpace; i++)
            {
                StringValue += CParse.Tab;
            }

            StringValue += CParse.RightScope;
        }

        public RightScopeSyntax(int tabspace, bool isClosed) : this(tabspace)
        {
            if (!isClosed)
            {
                StringValue += CParse.Comma;
                return;
            }
                
            StringValue += CParse.Semicolon;
        }

        public RightScopeSyntax()
        {
        }

        public override string ToString()
        {
            if (IsCommaNeeded)
                return StringValue + CParse.Comma + CParse.Paragraph;

            return StringValue + CParse.Paragraph;
        }
    }

    public class SyntaxClassDeclaration : ISyntax
    {
        public string Type;
        public int TabSpace;

        public string StringValue { get; set; } = "class";
        public List<ISyntax> Tree { get; set; } = new List<ISyntax>();

        public List<ISyntax> Modificators { get; } = new List<ISyntax>();

        public void AddTabulation(int tab)
        {

        }

        public override string ToString()
        {
            var data = string.Empty;

            for (int i = 0; i < TabSpace; i++)
                data += CParse.Tab;

            foreach (var m in Modificators)
                data += m.ToString() + CParse.Space;

            return data + StringValue + CParse.Space + Type + CParse.Paragraph;
        }
    }

    public class TabSimpleSyntax : ISyntax
    {
        public string StringValue { get; set; }
        public List<ISyntax> Tree { get; set; }

        private int index;

        public TabSimpleSyntax(int tab, string data)
        {
            StringValue = data;
            index = tab;
        }

        public override string ToString()
        {
            var data = string.Empty;

            for (int i = 0; i < index; i++)
            {
                data += CParse.Tab;
            }

            data += StringValue;
            data += CParse.Paragraph;

            return data;
        }
    }

    public class CompositeSyntax : ISyntax
    {
        public string StringValue { get; set; } = string.Empty;
        public List<ISyntax> Tree { get; set; } = new List<ISyntax>();

        public CompositeSyntax(ISyntax syntax, params ISyntax[] syntaxes)
        {
            Tree.Add(syntax);
            Tree.AddRange(syntaxes);
        }

        public CompositeSyntax(List<ISyntax> tree)
        {
            Tree = tree;
        }

        public override string ToString()
        {
            var stringZ = string.Empty;

            foreach (var z in Tree)
                stringZ += z.ToString();

            return stringZ;
        }
    }

    public class SpaceSyntax : ISyntax
    {
        public string StringValue { get; set; } = CParse.Space;
        public List<ISyntax> Tree { get; set; }

        public override string ToString()
        {
            return StringValue;
        }
    }

    public class ModificatorSyntax : ISyntax
    {
        public ModificatorSyntax()
        {
        }

        public ModificatorSyntax(string stringValue)
        {
            StringValue = stringValue;
        }

        public string StringValue { get; set; }
        public List<ISyntax> Tree { get; set; }

        public override string ToString()
        {
            return StringValue + CParse.Space;
        }
    }
}