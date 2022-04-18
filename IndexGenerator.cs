using System;

namespace HECSFramework.Core
{
    public static class IndexGenerator
    {
        public static int GenerateIndex(this string data)
        {
            return GetIndexForType(data);
        }

        public static int GetIndexForType(Type c)
        {
            var typeName = c.Name;
            return GetIndexForType(typeName);
        }

        public static int GetIndexForType(string typeName)
        {
            var lenght = typeName.Length;
            int index = lenght + typeName[0].GetHashCode();
            int hash = 10070531;

            for (int i = 0; i < lenght; i++)
            {
                int charC = typeName[i].GetHashCode()*i;
                index += (charC + (101161*(i+3)) + (hash ^ (charC*i)));
            }

            return index;
        }
    }
}