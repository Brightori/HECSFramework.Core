using System.Collections.Generic;

namespace HECSFramework.Core.Helpers
{
    public static class CollectionsExtentions
    {
        public static void AddOrReplace<T,U>(this Dictionary<T,U> dictionary, T key,  U value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        public static bool AddOrRemoveElement<T>(this List<T> list, T element, bool add)
        {
            if (add)
            {
                if (list.Contains(element))
                    return false;
                list.Add(element);
                return true;
            }

            if (list.Contains(element))
            {
                list.Remove(element);
                return true;
            }

            return false;
        }

        public static void AddOrRemoveElement<T>(this HashSet<T> list, T element, bool add)
        {
            if (add)
            {
                if (list.Contains(element))
                    return;
                list.Add(element);
            }
            else
                if (list.Contains(element))
                list.Remove(element);
        }

        public static T GetHECSComponent<T>(this List<IComponent> components) where T: IComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T needed)
                    return needed;
            }

            return default;
        }
    }
}