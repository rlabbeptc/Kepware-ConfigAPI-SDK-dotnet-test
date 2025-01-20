using KepwareSync.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static KepwareSync.EntityCompare;

namespace KepwareSync
{
    /// <summary>
    /// Compares two entities and determines if they are the same, different, or if one is missing.
    /// Left represents the source and Right represents the target.
    /// 
    /// PresentInRightOnly - The entity is in the Right but not in the Left.
    /// PresentInLeftOnly - The entity is in the Left but not in the Right.
    /// Changed - The entity is in both the Left and Right but the properties are different.
    /// None - The entity is in both the Left and Right and the properties are the same.
    /// </summary>
    public class EntityCompare
    {
        public enum CompareResult
        {
            PresentInLeftOnly = -1,
            None = 0,
            Changed = 1,
            PresentInRightOnly = 2,
        }

        public static CollectionResultBucket<T, K> Compare<T, K>(T? left, T? right)
            where T : EntityCollection<K>
            where K : NamedEntity
        {
            return Compare<T, K>(left, right, k => k.Name);
        }

        public static CollectionResultBucket<T, K> Compare<T, K>(T? left, T? right, Func<K, string> keySelector)
           where T : EntityCollection<K>
           where K : BaseEntity
        {
            var results = new List<ResultBucket<K>>();
            if (left == null && right == null)
            {
                return new CollectionResultBucket<T, K>(results);
            }
            else if (left == null)
            {
                results.AddRange(right!.Select(ResultBucket<K>.PresentInRight));
            }
            else if (right == null)
            {
                results.AddRange(left.Select(ResultBucket<K>.PresentInLeft));
            }
            else
            {
                var leftItems = left.ToDictionary(keySelector);
                var rightItems = right.ToDictionary(keySelector);
                Dictionary<long, K>? leftByUid = null, rightByUid = null;

                if (typeof(K).IsAssignableTo(typeof(NamedUidEntity)))
                {
                    leftByUid = left.Cast<NamedUidEntity>().Where(u => u.UniqueId != 0).ToDictionary(k => k.UniqueId, k => (K)(object)k);
                    rightByUid = right.Cast<NamedUidEntity>().Where(u => u.UniqueId != 0).ToDictionary(k => k.UniqueId, k => (K)(object)k);
                }

                foreach (var leftItem in left)
                {
                    if (rightItems.TryGetValue(keySelector(leftItem), out var rightItem))
                    {
                        results.Add(GetResult(leftItem, rightItem));

                    }
                    else if (leftItem is NamedUidEntity leftUidItem && rightByUid?.TryGetValue(leftUidItem.UniqueId, out var rightItemByUid) == true)
                    {
                        results.Add(GetResult(leftItem, rightItemByUid));
                    }
                    else
                    {
                        results.Add(ResultBucket<K>.PresentInLeft(leftItem));
                    }
                }

                foreach (var rightItem in right)
                {
                    if (!leftItems.ContainsKey(keySelector(rightItem)))
                    {
                        if (rightItem is NamedUidEntity rightUidItem && leftByUid?.TryGetValue(rightUidItem.UniqueId, out var leftItemByUid) == true)
                        {
                            //found by uuid
                        }
                        else
                        {
                            results.Add(ResultBucket<K>.PresentInRight(rightItem));
                        }
                    }
                }
            }
            var retValue = new CollectionResultBucket<T, K>(results);
#if DEBUG
            // do logical assertations based on the found item counts
            var leftCount = left?.Count ?? 0;
            var rightCount = right?.Count ?? 0;
            var addedCount = retValue.ItemsOnlyInRight.Count;
            var removedCount = retValue.ItemsOnlyInLeft.Count;
            var changedCount = retValue.ChangedItems.Count;
            var unchangedCount = retValue.UnchangedItems.Count;

            if (leftCount + addedCount - removedCount != rightCount)
            {
                throw new InvalidOperationException("The counts of the items are not correct.");
            }

            if (changedCount + unchangedCount != leftCount - removedCount)
            {
                throw new InvalidOperationException("The counts of the items are not correct.");
            }
#endif

            return retValue;
        }

        private static ResultBucket<K> GetResult<K>(K leftItem, K rightItem) where K : BaseEntity
        {
            return Compare(leftItem, rightItem) switch
            {
                CompareResult.PresentInRightOnly => ResultBucket<K>.PresentInRight(rightItem),
                CompareResult.PresentInLeftOnly => ResultBucket<K>.PresentInLeft(leftItem),
                CompareResult.Changed => ResultBucket<K>.Changed(leftItem, rightItem),
                _ => ResultBucket<K>.Unchanged(leftItem, rightItem),
            };
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
                return CompareResult.PresentInRightOnly;
            }
            else if (right == null)
            {
                return CompareResult.PresentInLeftOnly;
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
            /// <summary>
            /// The items that are in the Right but not in the Left.
            /// </summary>
            public ReadOnlyCollection<ResultBucket<K>> ItemsOnlyInRight { get; }
            /// <summary>
            /// The items that are in the Left but not in the Right.
            /// </summary>
            public ReadOnlyCollection<ResultBucket<K>> ItemsOnlyInLeft { get; }
            /// <summary>
            /// The items that are in both the Left and Right but the properties are different.
            /// </summary>
            public ReadOnlyCollection<ResultBucket<K>> ChangedItems { get; }
            /// <summary>
            /// The items that are in both the Left and Right and the properties are the same.
            /// </summary>
            public ReadOnlyCollection<ResultBucket<K>> UnchangedItems { get; }

            public CollectionResultBucket(IEnumerable<ResultBucket<K>> results)
            {
                ItemsOnlyInRight = results.Where(r => r.CompareResult == CompareResult.PresentInRightOnly).ToList().AsReadOnly();
                ItemsOnlyInLeft = results.Where(r => r.CompareResult == CompareResult.PresentInLeftOnly).ToList().AsReadOnly();
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

            public static ResultBucket<T> PresentInRight(T right)
               => new ResultBucket<T>
               {
                   Left = default,
                   Right = right,
                   CompareResult = CompareResult.PresentInRightOnly,
               };

            public static ResultBucket<T> PresentInLeft(T left)
                => new ResultBucket<T>
                {
                    Left = left,
                    Right = default,
                    CompareResult = CompareResult.PresentInLeftOnly,
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
