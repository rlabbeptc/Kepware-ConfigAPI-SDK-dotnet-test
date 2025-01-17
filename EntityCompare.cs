using KepwareSync.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KepwareSync.EntityCompare;

namespace KepwareSync
{
    /// <summary>
    /// Compares two entities and determines if they are the same, different, or if one is missing.
    /// Left represents the source and Right represents the target.
    /// 
    /// Added - The entity is in the Right but not in the Left.
    /// Removed - The entity is in the Left but not in the Right.
    /// Changed - The entity is in both the Left and Right but the properties are different.
    /// None - The entity is in both the Left and Right and the properties are the same.
    /// </summary>
    public class EntityCompare
    {
        public enum CompareResult
        {
            Removed = -1,
            None = 0,
            Changed = 1,
            Added = 2,
        }

        public static CollectionResultBucket<T, K> Compare<T, K>(T? left, T? right)
            where T : EntityCollection<K>
            where K : NamedEntity
        {
            var results = new List<ResultBucket<K>>();
            if (left == null && right == null)
            {
                return new CollectionResultBucket<T, K>(results);
            }
            else if (left == null)
            {
                results.AddRange(right!.Items.Select(ResultBucket<K>.Added));
            }
            else if (right == null)
            {
                results.AddRange(left.Items.Select(ResultBucket<K>.Removed));
            }
            else
            {
                var leftItems = left.Items.ToDictionary(i => i.Name);
                var rightItems = right.Items.ToDictionary(i => i.Name);
                foreach (var leftItem in left.Items)
                {
                    if (rightItems.TryGetValue(leftItem.Name, out var rightItem))
                    {
                        results.Add(
                            Compare(leftItem, rightItem) switch
                            {
                                CompareResult.Added => ResultBucket<K>.Added(rightItem),
                                CompareResult.Removed => ResultBucket<K>.Removed(leftItem),
                                CompareResult.Changed => ResultBucket<K>.Changed(leftItem, rightItem),
                                _ => ResultBucket<K>.Unchanged(leftItem, rightItem),
                            }
                        );

                    }
                    else
                    {
                        results.Add(ResultBucket<K>.Removed(leftItem));
                    }
                }
                foreach (var rightItem in right.Items)
                {
                    if (!leftItems.ContainsKey(rightItem.Name))
                    {
                        results.Add(ResultBucket<K>.Added(rightItem));
                    }
                }
            }
            return new CollectionResultBucket<T, K>(results);
        }

        public static CompareResult Compare<T>(T? left, T? right)
            where T : BaseEntity
        {
            if (left == null && right == null)
            {
                return CompareResult.None;
            }
            else if (left == null)
            {
                return CompareResult.Added;
            }
            else if (right == null)
            {
                return CompareResult.Removed;
            }
            else if (left.Equals(right))
            {
                return CompareResult.None;
            }
            else
            {
                return CompareResult.Changed;
            }
        }

        public class CollectionResultBucket<T, K>
            where T : EntityCollection<K>
            where K : BaseEntity
        {
            public ReadOnlyCollection<ResultBucket<K>> AddedItems { get; }
            public ReadOnlyCollection<ResultBucket<K>> RemovedItems { get; }
            public ReadOnlyCollection<ResultBucket<K>> ChangedItems { get; }
            public ReadOnlyCollection<ResultBucket<K>> UnchangedItems { get; }

            public CollectionResultBucket(IEnumerable<ResultBucket<K>> results)
            {
                AddedItems = results.Where(r => r.CompareResult == CompareResult.Added).ToList().AsReadOnly();
                RemovedItems = results.Where(r => r.CompareResult == CompareResult.Removed).ToList().AsReadOnly();
                ChangedItems = results.Where(r => r.CompareResult == CompareResult.Changed).ToList().AsReadOnly();
                UnchangedItems = results.Where(r => r.CompareResult == CompareResult.None && r.Left != null).ToList().AsReadOnly();
            }
        }

        [DebuggerDisplay("{CompareResult} - {LeftName ?? RightName}")]
        public record ResultBucket<T>
        {
            public T? Left { get; private set; }
            public T? Right { get; private set; }

            public CompareResult CompareResult { get; private set; }

            public string? LeftName => (Left as NamedEntity)?.Name;
            public string RightName => (Right as NamedEntity)?.Name ?? typeof(T).Name;

            public static ResultBucket<T> Added(T right)
               => new ResultBucket<T>
               {
                   Left = default,
                   Right = right,
                   CompareResult = CompareResult.Added,
               };

            public static ResultBucket<T> Removed(T left)
                => new ResultBucket<T>
                {
                    Left = left,
                    Right = default,
                    CompareResult = CompareResult.Removed,
                };

            public static ResultBucket<T> Changed(T left, T right)
                => new ResultBucket<T>
                {
                    Left = left,
                    Right = right,
                    CompareResult = CompareResult.Changed,
                };

            public static ResultBucket<T> Unchanged(T left, T right)
                => new ResultBucket<T>
                {
                    Left = left,
                    Right = right,
                    CompareResult = CompareResult.None,
                };
        }
    }
}
