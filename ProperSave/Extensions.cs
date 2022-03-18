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
    }
}
