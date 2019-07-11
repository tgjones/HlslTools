// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace ShaderTools.Utilities.Collections
{
    internal static partial class EnumerableExtensions
    {
        public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // perf optimization. try to not use enumerator if possible
            var list = source as IList<T>;
            if (list != null)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    action(list[i]);
                }
            }
            else
            {
                foreach (var value in source)
                {
                    action(value);
                }
            }

            return source;
        }

        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new ReadOnlyCollection<T>(source.ToList());
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T value)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.ConcatWorker(value);
        }

        private static IEnumerable<T> ConcatWorker<T>(this IEnumerable<T> source, T value)
        {
            foreach (var v in source)
            {
                yield return v;
            }

            yield return value;
        }

        public static ISet<T> ToSet<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source as ISet<T> ?? new HashSet<T>(source);
        }

        public static T? FirstOrNullable<T>(this IEnumerable<T> source)
            where T : struct
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Cast<T?>().FirstOrDefault();
        }

        public static bool IsSingle<T>(this IEnumerable<T> list)
        {
            using (var enumerator = list.GetEnumerator())
            {
                return enumerator.MoveNext() && !enumerator.MoveNext();
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            var readOnlyCollection = source as IReadOnlyCollection<T>;
            if (readOnlyCollection != null)
            {
                return readOnlyCollection.Count == 0;
            }

            var genericCollection = source as ICollection<T>;
            if (genericCollection != null)
            {
                return genericCollection.Count == 0;
            }

            var collection = source as ICollection;
            if (collection != null)
            {
                return collection.Count == 0;
            }

            var str = source as string;
            if (str != null)
            {
                return str.Length == 0;
            }

            foreach (var t in source)
            {
                return false;
            }

            return true;
        }

        public static bool IsEmpty<T>(this IReadOnlyCollection<T> source)
        {
            return source.Count == 0;
        }

        public static bool IsEmpty<T>(this ICollection<T> source)
        {
            return source.Count == 0;
        }

        private static readonly Func<object, bool> s_notNullTest = x => x != null;

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source)
            where T : class
        {
            if (source == null)
            {
                return SpecializedCollections.EmptyEnumerable<T>();
            }

            return source.Where((Func<T, bool>)s_notNullTest);
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> sequence)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            return sequence.SelectMany(s => s);
        }

        public static bool SequenceEqual<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> comparer)
        {
            Debug.Assert(comparer != null);

            if (first == second)
            {
                return true;
            }

            if (first == null || second == null)
            {
                return false;
            }

            using (var enumerator = first.GetEnumerator())
            using (var enumerator2 = second.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!enumerator2.MoveNext() || !comparer(enumerator.Current, enumerator2.Current))
                    {
                        return false;
                    }
                }

                if (enumerator2.MoveNext())
                {
                    return false;
                }
            }

            return true;
        }
    }
}