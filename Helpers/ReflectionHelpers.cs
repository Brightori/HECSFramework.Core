namespace HECSFramework.Core.Helpers
{
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

            var field = objectWithValue.GetType().GetField(nameOfField, bindingFlags);
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

            var field = objectWithValue.GetType().GetField(nameOfField, bindingFlags);
            field?.SetValue(objectWithValue, value);
        }
    }
}
