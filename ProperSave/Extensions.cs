using System;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

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

        public static int DifferenceCount<T>(this IEnumerable<T> collection, IEnumerable<T> second)
        {
            var secondCopy = second.ToList();
            var count = collection.Count();
            foreach (var element in collection)
            {
                if (secondCopy.Remove(element))
                {
                    count--;
                }
            }
            count += secondCopy.Count;

            return count;
        }
    }
}
