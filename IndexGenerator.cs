using System;

namespace HECSFramework.Core 
{
    public static class IndexGenerator
    {
        public static int GetIndexForType(Type c)
        {
            var typeName = c.Name;
            int index = typeName.Length + typeName[0].GetHashCode();
            int half = typeName.Length / 2;

            for (int i = 0; i < typeName.Length; i++)
            {
                char charC = typeName[i];
                index += charC.GetHashCode();
                index += typeName.Length;

                if (i > half)
                    index += -1531;

                if (charC == 'a')
                    index += -51;

                if (charC == 'o')
                    index += 33;

                if (charC == 'e')
                    index += -7;
            }

            return index;
        }
    }
}