﻿// Theraot: https://github.com/theraot/Theraot

#if LESSTHAN_NET30
#pragma warning disable CC0031 // Check for null before calling a delegate
#pragma warning disable RECS0017 // Possible compare of value type with 'null'
// ReSharper disable LoopCanBeConvertedToQuery

using System.Collections;
using System.Collections.Generic;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage( AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method )]
    public sealed class ExtensionAttribute : Attribute
    {
    }
}

public delegate TResult Func<TArg0, TResult>( TArg0 arg0 );
public delegate TResult Func<TArg0, TArg1, TResult>( TArg0 arg0, TArg1 arg1 );

namespace System.Linq
{
    public static partial class Enumerable
    {
        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var enumerator = source.GetEnumerator();
            using (enumerator)
            {
                if (!enumerator.MoveNext())
                {
                    throw new InvalidOperationException("No elements in source list");
                }

                var folded = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    folded = func(folded, enumerator.Current);
                }
                return folded;
            }
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var folded = seed;
            foreach (var item in source)
            {
                folded = func(folded, item);
            }
            return folded;
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        {
            if (resultSelector == null)
            {
                throw new ArgumentNullException(nameof(resultSelector));
            }
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var result = seed;
            foreach (var item in source)
            {
                result = func(result, item);
            }
            return resultSelector(result);
        }

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var enumerator = source.GetEnumerator();
            using (enumerator)
            {
                while (enumerator.MoveNext())
                {
                    if (!predicate(enumerator.Current))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            switch (source)
            {
                case null:
                    throw new ArgumentNullException(nameof(source));
                case ICollection<TSource> collection:
                    return collection.Count > 0;
                default:
                    break;
            }

            using (var enumerator = source.GetEnumerator())
            {
                return enumerator.MoveNext();
            }
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source)
        {
            return source;
        }

        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
        {
            switch (source)
            {
                case null:
                    throw new ArgumentNullException(nameof(source));
                case IEnumerable<TResult> enumerable:
                    return enumerable;
                default:
                    return CastExtracted();
            }

            IEnumerable<TResult> CastExtracted()
            {
                foreach (var obj in source)
                {
                    yield return (TResult)obj;
                }
            }
        }

        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }
            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }
            return ConcatExtracted();
            IEnumerable<TSource> ConcatExtracted()
            {
                foreach (var item in first)
                {
                    yield return item;
                }
                foreach (var item in second)
                {
                    yield return item;
                }
            }
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            return Contains(source, value, null);
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            foreach (var item in source)
            {
                if (comparer.Equals(item, value))
                {
                    return true;
                }
            }
            return false;
        }

        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            switch (source)
            {
                case null:
                    throw new ArgumentNullException(nameof(source));
                case ICollection<TSource> collection:
                    return collection.Count;
                default:
                    var result = 0;
                    using (var item = source.GetEnumerator())
                    {
                        while (item.MoveNext())
                        {
                            checked
                            {
                                result++;
                            }
                        }
                    }
                    return result;
            }
        }

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return Count(source.Where(predicate));
        }

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source)
        {
            var item = default(TSource);
            return DefaultIfEmpty(source, item);
        }

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return DefaultIfEmptyExtracted();

            IEnumerable<TSource> DefaultIfEmptyExtracted()
            {
                var enumerator = source.GetEnumerator();
                using (enumerator)
                {
                    if (enumerator.MoveNext())
                    {
                        do
                        {
                            yield return enumerator.Current;
                        }
                        while (enumerator.MoveNext());
                    }
                    else
                    {
                        yield return defaultValue;
                    }
                }
            }
        }

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source)
        {
            return Distinct(source, null);
        }

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return DistinctExtracted();

            IEnumerable<TSource> DistinctExtracted()
            {
                var found = new Dictionary<TSource, object>(comparer);
                var foundNull = false;
                foreach (var item in source)
                {
                    // item might be null
                    if (item == null)
                    {
                        if (foundNull)
                        {
                            continue;
                        }
                        foundNull = true;
                    }
                    else
                    {
                        if (found.ContainsKey(item))
                        {
                            continue;
                        }
                        found.Add(item, null);
                    }
                    yield return item;
                }
            }
        }

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "index < 0");
            }
            switch (source)
            {
                case IList<TSource> list:
                    return list[index];
                default:
                    var count = 0L;
                    foreach (var item in source)
                    {
                        if (index == count)
                        {
                            return item;
                        }
                        count++;
                    }
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (index < 0)
            {
                return default( TSource );
            }
            switch (source)
            {
                case IList<TSource> list:
                    return index < list.Count ? list[index] : default( TSource );
                default:
                    var count = 0L;
                    foreach (var item in source)
                    {
                        if (index == count)
                        {
                            return item;
                        }
                        count++;
                    }
                    return default( TSource );
            }
        }

        public static IEnumerable<TResult> Empty<TResult>()
        {
            yield break;
        }


        public static TSource First<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (!(source is IList<TSource> list))
            {
                using (var enumerator = source.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        return enumerator.Current;
                    }
                }
            }
            else
            {
                if (list.Count != 0)
                {
                    return list[0];
                }
            }

            throw new InvalidOperationException("The source sequence is empty");
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return item;
                }
            }
            throw new InvalidOperationException();
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            foreach (var item in source)
            {
                return item;
            }
            return default( TSource );
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return FirstOrDefault(source.Where(predicate));
        }


        public static TSource Last<TSource>(this IEnumerable<TSource> source)
        {
            switch (source)
            {
                case null:
                    throw new ArgumentNullException(nameof(source));
                case ICollection<TSource> collection when collection.Count == 0:
                    throw new InvalidOperationException();
                case IList<TSource> list:
                    return list[list.Count - 1];
                default:
                    var found = false;
                    var result = default(TSource);
                    foreach (var item in source)
                    {
                        result = item;
                        found = true;
                    }
                    if (found)
                    {
                        return result;
                    }
                    throw new InvalidOperationException();
            }
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var found = false;
            var result = default(TSource);
            foreach (var item in source)
            {
                if (!predicate(item))
                {
                    continue;
                }
                result = item;
                found = true;
            }
            if (found)
            {
                return result;
            }

            throw new InvalidOperationException();
        }

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            switch (source)
            {
                case null:
                    throw new ArgumentNullException(nameof(source));
                case IList<TSource> list:
                    return list.Count > 0 ? list[list.Count - 1] : default( TSource );
                default:
                    var found = false;
                    var result = default(TSource);
                    foreach (var item in source)
                    {
                        result = item;
                        found = true;
                    }
                    return found ? result : default( TSource );
            }
        }

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var result = default(TSource);
            foreach (var item in source)
            {
                if (!predicate(item))
                {
                    continue;
                }
                result = item;
            }
            return result;
        }

        public static long LongCount<TSource>(this IEnumerable<TSource> source)
        {
            switch (source)
            {
                case null:
                    throw new ArgumentNullException(nameof(source));
                case TSource[] array:
                    return array.LongLength;
                default:
                    long count = 0;
                    using (var item = source.GetEnumerator())
                    {
                        while (item.MoveNext())
                        {
                            count++;
                        }
                    }
                    return count;
            }
        }

        public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return LongCount(source.Where(predicate));
        }

        public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return OfTypeExtracted();

            IEnumerable<TResult> OfTypeExtracted()
            {
                foreach (var item in source)
                {
                    if (item is TResult result)
                    {
                        yield return result;
                    }
                }
            }
        }


        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "count < 0");
            }

            return RepeatExtracted();

            IEnumerable<TResult> RepeatExtracted()
            {
                for (var index = 0; index < count; index++)
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return ReverseExtracted();

            IEnumerable<TSource> ReverseExtracted()
            {
                var stack = new Stack<TSource>();
                foreach (var item in source)
                {
                    stack.Push(item);
                }
                foreach (var item in stack)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return Select(source, (item, _) => selector(item));
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            return SelectExtracted(source, selector);
        }

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            return SelectManyExtracted();

            IEnumerable<TResult> SelectManyExtracted()
            {
                foreach (var key in source)
                {
                    foreach (var item in selector(key))
                    {
                        yield return item;
                    }
                }
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            return SelectManyExtracted();

            IEnumerable<TResult> SelectManyExtracted()
            {
                var count = 0;
                foreach (var key in source)
                {
                    foreach (var item in selector(key, count))
                    {
                        yield return item;
                    }
                    count++;
                }
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (collectionSelector == null)
            {
                throw new ArgumentNullException(nameof(collectionSelector));
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException(nameof(resultSelector));
            }
            return SelectManyExtracted();

            IEnumerable<TResult> SelectManyExtracted()
            {
                foreach (var element in source)
                {
                    foreach (var collection in collectionSelector(element))
                    {
                        yield return resultSelector(element, collection);
                    }
                }
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (collectionSelector == null)
            {
                throw new ArgumentNullException(nameof(collectionSelector));
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException(nameof(resultSelector));
            }
            return SelectManyExtracted();

            IEnumerable<TResult> SelectManyExtracted()
            {
                var count = 0;
                foreach (var element in source)
                {
                    foreach (var collection in collectionSelector(element, count))
                    {
                        yield return resultSelector(element, collection);
                    }
                    count++;
                }
            }
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            return SequenceEqual(first, second, null);
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }
            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            return SequenceEqualExtracted();

            bool SequenceEqualExtracted()
            {
                using (IEnumerator<TSource> firstEnumerator = first.GetEnumerator(), secondEnumerator = second.GetEnumerator())
                {
                    while (firstEnumerator.MoveNext())
                    {
                        if (!secondEnumerator.MoveNext())
                        {
                            return false;
                        }
                        if (!comparer.Equals(firstEnumerator.Current, secondEnumerator.Current))
                        {
                            return false;
                        }
                    }
                    return !secondEnumerator.MoveNext();
                }
            }
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var found = false;
            var result = default(TSource);
            foreach (var item in source)
            {
                if (found)
                {
                    throw new InvalidOperationException();
                }
                found = true;
                result = item;
            }
            if (found)
            {
                return result;
            }
            throw new InvalidOperationException();
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var found = false;
            var result = default(TSource);
            foreach (var item in source)
            {
                if (!predicate(item))
                {
                    continue;
                }
                if (found)
                {
                    throw new InvalidOperationException();
                }
                found = true;
                result = item;
            }
            if (found)
            {
                return result;
            }
            throw new InvalidOperationException();
        }

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var found = false;
            var result = default(TSource);
            foreach (var item in source)
            {
                if (found)
                {
                    throw new InvalidOperationException();
                }
                found = true;
                result = item;
            }
            return result;
        }

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var found = false;
            var result = default(TSource);
            foreach (var item in source)
            {
                if (!predicate(item))
                {
                    continue;
                }
                if (found)
                {
                    throw new InvalidOperationException();
                }
                found = true;
                result = item;
            }
            return result;
        }

        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
        {
            return SkipWhile(source, (_, i) => i < count);
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return SkipWhile(source, (item, _) => predicate(item));
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return SkipWhileExtracted();

            IEnumerable<TSource> SkipWhileExtracted()
            {
                var enumerator = source.GetEnumerator();
                using (enumerator)
                {
                    var count = 0;
                    while (enumerator.MoveNext())
                    {
                        if (!predicate(enumerator.Current, count))
                        {
                            while (true)
                            {
                                yield return enumerator.Current;
                                if (!enumerator.MoveNext())
                                {
                                    yield break;
                                }
                            }
                        }
                        count++;
                    }
                }
            }
        }

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return TakeWhileExtracted();

            IEnumerable<TSource> TakeWhileExtracted()
            {
                if (count > 0)
                {
                    var currentCount = 0;
                    foreach (var item in source)
                    {
                        yield return item;
                        currentCount++;
                        if (currentCount == count)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return TakeWhile(source, (item, _) => predicate(item));
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return TakeWhileExtracted();

            IEnumerable<TSource> TakeWhileExtracted()
            {
                var count = 0;
                foreach (var item in source)
                {
                    if (!predicate(item, count))
                    {
                        break;
                    }
                    yield return item;
                    count++;
                }
            }
        }


        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
        {
            switch (source)
            {
                case null:
                    throw new ArgumentNullException(nameof(source));
                case ICollection<TSource> collection:
                {
                    var result = new TSource[collection.Count];
                    collection.CopyTo(result, 0);
                    return result;
                }
                default:
                    return new List<TSource>(source).ToArray();
            }
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return ToDictionary(source, keySelector, elementSelector, null);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (elementSelector == null)
            {
                throw new ArgumentNullException(nameof(elementSelector));
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }
            comparer = comparer ?? EqualityComparer<TKey>.Default;
            var result = new Dictionary<TKey, TElement>(comparer);
            foreach (var item in source)
            {
                result.Add(keySelector(item), elementSelector(item));
            }
            return result;
        }


        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
        {
            if ( source == null )
            {
                throw new ArgumentNullException( nameof( source ) );
            }

            var str = source as string;

            if ( str == null )
            {
                return new List<TSource>( source );
            }

            var array = ( TSource[] ) ( object ) str.ToCharArray();

            var result = new List<TSource>( array.Length );

            result.AddRange( array );

            return result;
        }


        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            return Union(first, second, null);
        }

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            return Distinct(Concat(first, second), comparer);
        }

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return Where(source, (item, _) => predicate(item));
        }

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return WhereExtracted();

            IEnumerable<TSource> WhereExtracted()
            {
                var count = 0;
                foreach (var item in source)
                {
                    if (!predicate(item, count))
                    {
                        continue;
                    }
                    yield return item;
                    count++;
                }
            }
        }

        public static IEnumerable<TReturn> Zip<T1, T2, TReturn>(this IEnumerable<T1> first, IEnumerable<T2> second, Func<T1, T2, TReturn> resultSelector)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }
            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException(nameof(resultSelector));
            }
            return ZipExtracted();

            IEnumerable<TReturn> ZipExtracted()
            {
                using (var enumeratorFirst = first.GetEnumerator())
                using (var enumeratorSecond = second.GetEnumerator())
                {
                    while
                    (
                        enumeratorFirst.MoveNext()
                        && enumeratorSecond.MoveNext()
                    )
                    {
                        yield return resultSelector
                        (
                            enumeratorFirst.Current,
                            enumeratorSecond.Current
                        );
                    }
                }
            }
        }


        private static IEnumerable<TResult> SelectExtracted<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        {
            // NOTICE this method has no null check
            var count = 0;
            foreach (var item in source)
            {
                yield return selector(item, count);
                count++;
            }
        }
    }
}

#endif