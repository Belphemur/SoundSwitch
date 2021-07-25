using System.Collections.Generic;
using System.Linq;

namespace SoundSwitch.Util.Extension
{
    public static class EnumerableExtension
    {
        public static IEnumerable<TSource> IntersectInverse<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {

            return IntersectIteratorInverse(first, second, null);
        }

        public static IEnumerable<TSource> IntersectInverse<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
        {

            return IntersectIteratorInverse(first, second, comparer);
        }

        private static IEnumerable<TSource> IntersectIteratorInverse<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
        {
            var set = second.ToDictionary(source => source, comparer);

            foreach (var element in first)
            {
                if (set.TryGetValue(element, out var secondElement))
                {
                    yield return secondElement;
                }
            }
        }
    }
}