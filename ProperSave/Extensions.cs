using System;
using System.Reflection;

namespace ProperSave
{
    public static class Extensions
    {
        public static T GetPrivateStaticField<T>(this Type type, string field)
        {
            return (T)type.GetField(field, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        public static void SetPrivateStaticField(this Type type, string field, object value)
        {
            type.GetField(field, BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, value);
        }
    }
}
