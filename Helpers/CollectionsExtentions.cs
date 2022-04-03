using System;
using System.Collections.Generic;

namespace HECSFramework.Core.Helpers
{
    public static class CollectionsExtentions
    {
        public static void AddOrReplace<T, U>(this Dictionary<T, U> dictionary, T key, U value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        public static void AddUniqueElement<T>(this List<T> list, T element)
        {
            if (list.Contains(element)) return;

            list.Add(element);
        }

        public static void AddOrGet<T, U>(this Dictionary<T, U> dictionary, T key, out U value) where U : new()
        {
            if (dictionary.ContainsKey(key))
                value = dictionary[key];
            else
            {
                var data = new U();
                value = data;
                dictionary.Add(key, data);
            }
        }

        public static void AddOrGet<T, U, Z>(this Dictionary<T, Z> dictionary, T key, out Z value) where Z : List<U>, new()
        {
            if (dictionary.ContainsKey(key))
                value = dictionary[key];
            else
            {
                var data = new Z();
                value = data;
                dictionary.Add(key, data);
            }
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

        public static void AddOrRemoveElement<T>(this ConcurrencyList<T> list, T element, bool add)
        {
            if (add)
            {
                if (list.Contains(element))
                    return;

                list.Add(element);
            }
            else
            {
                list.Remove(element);
            }
        }

        public static void RemoveElement<T>(this ConcurrencyList<T> list, Func<T, bool> predicate)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var element = list.Data[i];
                if (element == null) continue;

                if (predicate(element)) list.RemoveAt(i--);
            }
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

        public static T GetHECSComponent<T>(this List<IComponent> components) where T : IComponent
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