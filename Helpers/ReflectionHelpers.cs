using System;
using System.Reflection;

namespace HECSFramework.Core.Helpers
{
    [Documentation(Doc.Helpers, Doc.HECS, "this component provide helpers to reflection functionality like set private member or call method")]
    public static class ReflectionHelpers
    {
        public static void CallPrivateMethod(object invokeMethodObject, string methodName, params object[] args)
        {
            var mi = invokeMethodObject.GetType().GetMethod(methodName,
                 System.Reflection.BindingFlags.NonPublic
               | System.Reflection.BindingFlags.Public
               | System.Reflection.BindingFlags.Instance
               | System.Reflection.BindingFlags.Static
               | System.Reflection.BindingFlags.FlattenHierarchy);

            if (mi != null)
                mi.Invoke(invokeMethodObject, args);
        }

        public static T GetPrivateFieldValue<T>(object objectWithValue, string nameOfField)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags =
                 System.Reflection.BindingFlags.NonPublic
               | System.Reflection.BindingFlags.Public
               | System.Reflection.BindingFlags.Instance
               | System.Reflection.BindingFlags.Static
               | System.Reflection.BindingFlags.FlattenHierarchy;

            var field = GetFieldRecursive(objectWithValue.GetType(), nameOfField, bindingFlags);
            return (T)field?.GetValue(objectWithValue);
        }

        public static void SetPrivateFieldValue(object objectWithValue, string nameOfField, object value)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags =
                 System.Reflection.BindingFlags.NonPublic
               | System.Reflection.BindingFlags.Public
               | System.Reflection.BindingFlags.Instance
               | System.Reflection.BindingFlags.Static
               | System.Reflection.BindingFlags.FlattenHierarchy;

            var field = GetFieldRecursive(objectWithValue.GetType(), nameOfField, bindingFlags);
            field?.SetValue(objectWithValue, value);
        }

        public static FieldInfo GetFieldRecursive(Type type, string fieldName, BindingFlags flags)
        {
            FieldInfo field = null;

            while (type != null)
            {
                field = type.GetField(fieldName, flags);
                if (field != null)
                    return field;

                type = type.BaseType;
            }

            return null;
        }

        public static void SetPrivatePropertyValue(object objectWithValue, string nameOfField, object value)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags =
                 System.Reflection.BindingFlags.NonPublic
               | System.Reflection.BindingFlags.Public
               | System.Reflection.BindingFlags.Instance
               | System.Reflection.BindingFlags.Static
               | System.Reflection.BindingFlags.FlattenHierarchy;

            var field = objectWithValue.GetType().GetProperty(nameOfField, bindingFlags);
            field?.SetValue(objectWithValue, value);
        }
    }
}
