using System.Collections.Generic;
using System.Linq;

namespace ProperSave
{
    public static class Extensions
    {
        public static int DifferenceCount<T>(this IEnumerable<T> collection, IEnumerable<T> second)
        {
            var secondCopy = second.ToList();
            var count = 0;
            foreach (var element in collection)
            {
                if (!secondCopy.Remove(element))
                {
                    count++;
                }
            }
            count += secondCopy.Count;

            return count;
        }

        public static int AddOrIndexOf<T>(this List<T> list, T value)
        {
            var index = list.IndexOf(value);
            if (index < 0)
            {
                list.Add(value);
                return list.Count - 1;
            }

            return index;
        }

        public static T GetSafe<T>(this List<T> list, int index)
        {
            if (list is null || list.Count < (uint)index)
            {
                return default;
            }

            return list[index];
        }
    }
}
