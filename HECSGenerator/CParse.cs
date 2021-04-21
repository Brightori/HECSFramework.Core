using System.Collections.Generic;

namespace HECSFramework.Core.Generator
{
    public static class CParse
    {
        public const string Using = "using";
        public const string Void = "void";
        public const string Class = "class";
        public const string Public = "public";
        public const string Private = "private";
        public const string Static = "static";
        public const string Namespace = "namespace";
        public const string Paragraph = "\r";
        public const string Space = " ";
        public const string LeftScope = "{";
        public const string RightScope = "}";
        public const string Tab = "\t";
        public const string Semicolon = ";";
        public const string Struct = "struct";
        public const string Return = "return";
        public const string Comma = ",";
        public const string Quote = @"""";
        public const string Dot = @".";

        public static string[] Modifiers = new string[]
        {
            Public,
            Private,
            Static,
        };

        public static ISyntax ExtractModifier(ref string data)
        {
            foreach (var m in Modifiers)
            {
                if (data.Contains(m))
                {
                    data = data.Replace(m, "");
                    return new ModificatorSyntax(m);
                }
            }

            return default;
        }

        public static bool IsContainModificator(string data)
        {
            return data.Contains(Public) || data.Contains(Private) || data.Contains(Static);
        }

        public static string GetObjectRecursive(string data, IRawSyntaxData rawSyntaxData)
        {
            if (data.Contains(Semicolon))
                return data;

            var currentIndex = 0;

            for (int i = 0; i < rawSyntaxData.RawData.Length; i++)
            {
                if (data == rawSyntaxData.RawData[i])
                {
                    currentIndex = i;
                    break;
                }
            }

            string overallData = string.Empty;
            overallData += data;
            rawSyntaxData.RawData[currentIndex] = null;

            return GetObjectRecursive(ref overallData, ++currentIndex, rawSyntaxData);
        }

        private static string GetObjectRecursive(ref string currentData, int currentIndex, IRawSyntaxData rawSyntaxData)
        {
            if (currentIndex >= rawSyntaxData.RawData.Length)
                return currentData;

            currentData += rawSyntaxData.RawData[currentIndex];

            if (rawSyntaxData.RawData[currentIndex].Contains(";"))
            {
                rawSyntaxData.RawData[currentIndex] = null;
                return currentData;
            }
            else
            {
                rawSyntaxData.RawData[currentIndex] = null;
                return GetObjectRecursive(ref currentData, ++currentIndex, rawSyntaxData);
            }
        }
    }

    public static class FieldMembers
    {
        //сюда добавляем те типы которые надо отслеживать
        private static List<string> KnownMembers = new List<string>
        {
            "Dictionary",
            "List",
            "string",
            "int",
            "ComponentsMask",
            "ulong",
        };

        public static bool TryGetKnownType(string data, out string type)
        {
            foreach (var m in KnownMembers)
            {
                if (data.Contains(m))
                {
                    type = m;
                    return true;
                }
            }

            type = string.Empty;
            return false;
        }
    }
}
