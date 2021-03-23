using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HECSFramework.Core.Generator
{
    class HECSParserExperimental
    {
        /*private static string test = @"using System.Collections.Generic;

namespace HECSv2
{
    public static class TypesMap
    {
        public static Dictionary<string, ComponentsMask> mapIndexes = new Dictionary<string, ComponentsMask>
        {
            {""Health"", new ComponentsMask
                {
                Mask01 = 1 << 1,
                Mask02 = 1 << 0,
                }
            }
        };
    }
}
";*/

       

        public static void Parse(string data)
        {
            var split = data.Split('\n');
            var main = new TreeSyntaxNode();
            main.RawData = split;

            foreach (var s in split)
            {
                if (string.IsNullOrEmpty(s))
                    continue;

                main.FlatRoot.Add(GetNode(s, main));
            }

            var result = GetSyntaxOutput(main.FlatRoot);
            var path = Application.dataPath + "\\test.cs";

            //if (!File.Exists(path))
            //    File.Create(path);

            //File.WriteAllText(path, result, Encoding.UTF8);
        }

        public static string GetSyntaxOutput(IEnumerable<ISyntax> syntaxes)
        {
            var builder = new StringBuilder();

            foreach (var synt in syntaxes)
            {
                builder.Append(synt.ToString());
            }

            return builder.ToString();
        }

        public static ISyntax GetNode(string data, IRawSyntaxData rawSyntaxData, bool ignoreObjects = true)
        {
            if (data.Contains(CParse.Using) && !data.Contains("("))
            {
                var d = data.Replace(CParse.Using, "");
                var z = d.Replace("\r", "");
                var trim = z.Trim();
                return new UsingSyntax(trim);
            }

            if (data.Contains(CParse.Namespace))
            {
                var d = data.Replace(CParse.Namespace, "").Replace(CParse.Paragraph, "").Trim();
                return new NameSpaceSyntax(d);
            }

            if (data.Equals(CParse.Paragraph))
            {
                return new ParagraphSyntax();
            }

            if (data.Contains("{") && data.Replace("\r", "").Trim().Equals(CParse.LeftScope))
            {
                var lenght = (int)((data.Length - 2) / 3);
                return new LeftScopeSyntax(lenght);
            }

            if (data.Trim().Equals(CParse.RightScope))
            {
                var lenght = (int)((data.Length - 2) / 3);
                return new RightScopeSyntax(lenght);
            }

            if (data.Contains(CParse.Class))
            {
                var parametrs = data.Split(' ');
                var classDeclaration = new ClassDeclarationSyntax();

                for (int i = 0; i < parametrs.Length; i++)
                {
                    string p = parametrs[i];

                    if (p == " ")
                        continue;

                    if (CParse.IsContainModificator(p))
                        classDeclaration.Modificators.Add(new ModificatorSyntax(p));

                    var previous = i - 1;

                    if (previous >= 0 && parametrs[previous].Trim() == CParse.Class && p.Trim() != string.Empty)
                    {
                        classDeclaration.Type = p.Replace("\r", "");
                    }
                }

                var spaces = data.Length - data.TrimStart().Length;
                var tabCount = (int)(spaces / 4);

                return new CompositeSyntax(new TabSpaceSyntax(tabCount), classDeclaration);
            }

            if (FieldMembers.TryGetKnownType(data, out string type) && ignoreObjects)
            {
                var newField = new FieldMember(type);
                var objectT = CParse.GetObjectRecursive(data, rawSyntaxData);
                var split = objectT.Split('\r');
                newField.RawData = split;

                if (split != default && split.Length >= 0)
                {
                    newField.Tree.Add(new TabSpaceSyntax(CalculateTabs(split[0])));
                    split[0] = split[0].Trim();
                }


                objectT = objectT.Trim();

                //extract modifiers
                newField.Tree.Add(ExtractModifiers(ref objectT));

                var neededType = GetWordBefore(objectT.Trim(), '<', ' ');
                RemoveFirstKindOfWord(ref objectT, neededType);
            }

            return new SimpleSyntax(data);
        }

        public static void RemoveFirstKindOfWord(ref string data, string word)
        {
            var test = data.IndexOf(word);
            data = data.Remove(2, word.Length).Trim();
        }

        public static string GetWordBefore(string data, char before, params char[] additional)
        {
            var list = new List<char>(32);

            foreach (var x in data)
            {
                if (x == before || additional.Any(z => z == x))
                    break;

                list.Add(x);
            }

            return new string(list.ToArray());
        }

        public static ISyntax ExtractModifiers(ref string data)
        {
            var modifiers = new List<ISyntax>();

            while (CParse.IsContainModificator(data))
            {
                var modifier = CParse.ExtractModifier(ref data);

                if (modifier != null)
                {
                    modifiers.Add(modifier);
                    modifiers.Add(new SpaceSyntax());
                }

            }

            return new CompositeSyntax(modifiers);
        }

        public static int CalculateTabs(string data)
        {
            var spaces = data.Length - data.TrimStart().Length;
            return (int)(spaces / 4);
        }
    }
}