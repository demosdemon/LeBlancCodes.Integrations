using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;

namespace LeBlancCodes.Common.Extensions
{
    /// <summary>
    ///     Extensions for enumerable collections
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Yields only the non-null values in the enumerable.
        /// </summary>
        /// <typeparam name="T">The collection type</typeparam>
        /// <param name="source">The collection</param>
        /// <returns>An iterator over the non-null values in <paramref name="source" /></returns>
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source) where T : struct
        {
            if (source == null) yield break;

            foreach (var item in source.Where(item => item.HasValue))
                yield return item.Value;
        }

        /// <summary>
        ///     Yields only the non-null objects in the enumerable.
        /// </summary>
        /// <typeparam name="T">The collection type</typeparam>
        /// <param name="source">The collection</param>
        /// <returns>An iterator over the non-null values in <paramref name="source" /></returns>
        [SuppressMessage("ReSharper", "CompareNonConstrainedGenericWithNull")]
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source)
        {
            source = source ?? Enumerable.Empty<T>();
            var type = typeof(T);

            return type.IsNullable() ? source.Where(item => item != null) : source;
        }

        public static IEnumerable<T> Chain<T>(this IEnumerable<T> source, params IEnumerable<T>[] chainElements) =>
            chainElements.Aggregate(source ?? Enumerable.Empty<T>(), (current, prev) => prev.Concat(current ?? Enumerable.Empty<T>()));

        [ContractAnnotation(" => notnull")]
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source) => source ?? Enumerable.Empty<T>();
    }
}
