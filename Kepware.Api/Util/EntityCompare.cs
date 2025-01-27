using Kepware.Api.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.Api.Util
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
        /// <summary>
        /// The result of comparing two entities.
        /// </summary>
        public enum CompareResult
        {
            /// <summary>
            /// The entity is in the Left but not in the Right.
            /// </summary>
            PresentInLeftOnly = -1,
            /// <summary>
            /// The entity is in both the Left and Right and the properties are the same.
            /// </summary>
            None = 0,
            /// <summary>
            /// The entity is in both the Left and Right but the properties are different.
            /// </summary>
            Changed = 1,
            /// <summary>
            /// The entity is in the Right but not in the Left.
            /// </summary>
            PresentInRightOnly = 2,
        }

        /// <summary>
        /// Compares two entities and returns the comparison result.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="left">The left entity.</param>
        /// <param name="right">The right entity.</param>
        /// <returns>The comparison result.</returns>
        public static CompareResult Compare<T>(T? left, T? right)
            where T : NamedEntity
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
            else if (left.GetUpdateDiff(right, false).Count <= 0)
            {
                //Nothing to update
                return CompareResult.None;
            }
            else
            {
                return CompareResult.Changed;
            }
        }

        /// <summary>
        /// Compares two collections of entities and returns the comparison results.
        /// </summary>
        /// <typeparam name="T">The type of the entity collection.</typeparam>
        /// <typeparam name="K">The type of the entity.</typeparam>
        /// <param name="left">The left entity collection.</param>
        /// <param name="right">The right entity collection.</param>
        /// <returns>The comparison results.</returns>
        public static CollectionResultBucket<K> Compare<T, K>(T? left, T? right)
            where T : EntityCollection<K>
            where K : NamedEntity
        {
            return Compare<T, K>(left, right, k => k.Name);
        }

        /// <summary>
        /// Compares two collections of entities and returns the comparison results.
        /// </summary>
        /// <typeparam name="T">The type of the entity collection.</typeparam>
        /// <typeparam name="K">The type of the entity.</typeparam>
        /// <param name="left">The left entity collection.</param>
        /// <param name="right">The right entity collection.</param>
        /// <param name="keySelector">The function to select the key for comparison.</param>
        /// <returns>The comparison results.</returns>
        public static CollectionResultBucket<K> Compare<T, K>(T? left, T? right, Func<K, string> keySelector)
           where T : EntityCollection<K>
           where K : NamedEntity
        {
            var results = new List<ResultBucket<K>>();
            if (left == null && right == null)
            {
                return new CollectionResultBucket<K>(results);
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
                Dictionary<string, K> leftItems, rightItems;
                Dictionary<long, K>? leftByUid = null, rightByUid = null;

                if (typeof(K).IsAssignableTo(typeof(NamedUidEntity)))
                {
                    leftItems = new Dictionary<string, K>(left.Count);
                    rightItems = new Dictionary<string, K>(right.Count);
                    leftByUid = new Dictionary<long, K>(left.Count);
                    rightByUid = new Dictionary<long, K>(right.Count);

                    foreach (var rightItem in right)
                    {
                        rightItems.Add(keySelector(rightItem), rightItem);
                        if (rightItem is NamedUidEntity rightUidItem)
                        {
                            if (rightByUid.TryGetValue(rightUidItem.UniqueId, out var collisionItem) && collisionItem is NamedUidEntity collisionUidItem)
                            {
                                throw new InvalidOperationException($"Collision in unique id found on the right side: {rightItem.TypeName} UniqueId {rightUidItem.UniqueId} with name {rightUidItem.Name} collides with {collisionUidItem.Name}");
                            }
                            else
                            {
                                rightByUid[rightUidItem.UniqueId] = rightItem;
                            }
                        }
                    }

                    foreach (var leftItem in left)
                    {
                        leftItems.Add(keySelector(leftItem), leftItem);

                        if (leftItem is NamedUidEntity leftUidItem)
                        {
                            if (leftByUid.TryGetValue(leftUidItem.UniqueId, out var collisionItem) && collisionItem is NamedUidEntity collisionUidItem)
                            {
                                if (rightByUid.TryGetValue(leftUidItem.UniqueId, out var rightItemByUid) && rightItemByUid is NamedUidEntity rightUidItem)
                                {
                                    if (rightUidItem.Name == collisionUidItem.Name)
                                    {
                                        leftUidItem.RemoveUniqueId();
                                    }
                                    else if (rightUidItem.Name == leftUidItem.Name)
                                    {
                                        collisionUidItem.RemoveUniqueId();
                                    }
                                    else
                                    {
                                        collisionUidItem.RemoveUniqueId();
                                        leftUidItem.RemoveUniqueId();
                                    }
                                }
                                else
                                {
                                    throw new InvalidOperationException($"Collision in unique id found on the left side: {leftItem.TypeName} UniqueId {leftUidItem.UniqueId} with name {leftUidItem.Name}");
                                }
                            }
                            else
                            {
                                leftByUid[leftUidItem.UniqueId] = leftItem;
                            }
                        }
                    }
                }
                else
                {
                    leftItems = left.ToDictionary(keySelector);
                    rightItems = right.ToDictionary(keySelector);
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
                        if (rightItem is NamedUidEntity rightUidItem && leftByUid?.TryGetValue(rightUidItem.UniqueId, out var _) == true)
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
            var retValue = new CollectionResultBucket<K>(results);
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

        /// <summary>
        /// Gets the result of comparing two entities.
        /// </summary>
        /// <typeparam name="K">The type of the entity.</typeparam>
        /// <param name="leftItem">The left entity.</param>
        /// <param name="rightItem">The right entity.</param>
        /// <returns>The result of the comparison.</returns>
        private static ResultBucket<K> GetResult<K>(K leftItem, K rightItem) where K : NamedEntity
        {
            return Compare(leftItem, rightItem) switch
            {
                CompareResult.PresentInRightOnly => ResultBucket<K>.PresentInRight(rightItem),
                CompareResult.PresentInLeftOnly => ResultBucket<K>.PresentInLeft(leftItem),
                CompareResult.Changed => ResultBucket<K>.Changed(leftItem, rightItem),
                _ => ResultBucket<K>.Unchanged(leftItem, rightItem),
            };
        }

        /// <summary>
        /// Represents the result of comparing two collections of entities.
        /// </summary>
        /// <typeparam name="K">The type of the entity.</typeparam>
        public class CollectionResultBucket<K>
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

            /// <summary>
            /// Initializes a new instance of the <see cref="CollectionResultBucket{K}"/> class.
            /// </summary>
            /// <param name="results">The comparison results.</param>
            public CollectionResultBucket(IEnumerable<ResultBucket<K>> results)
            {
                ItemsOnlyInRight = results.Where(r => r.CompareResult == CompareResult.PresentInRightOnly).ToList().AsReadOnly();
                ItemsOnlyInLeft = results.Where(r => r.CompareResult == CompareResult.PresentInLeftOnly).ToList().AsReadOnly();
                ChangedItems = results.Where(r => r.CompareResult == CompareResult.Changed).ToList().AsReadOnly();
                UnchangedItems = results.Where(r => r.CompareResult == CompareResult.None && r.Left != null).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Represents the result of comparing two entities.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        [DebuggerDisplay("{CompareResult} - {LeftName ?? RightName}")]
        public record ResultBucket<T>
        {
            /// <summary>
            /// Gets the left entity.
            /// </summary>
            public T? Left { get; private set; }
            /// <summary>
            /// Gets the right entity.
            /// </summary>
            public T? Right { get; private set; }

            /// <summary>
            /// Gets the comparison result.
            /// </summary>
            public CompareResult CompareResult { get; private set; }

            /// <summary>
            /// Gets the name of the left entity.
            /// </summary>
            public string? LeftName => (Left as NamedEntity)?.Name;
            /// <summary>
            /// Gets the name of the right entity.
            /// </summary>
            public string RightName => (Right as NamedEntity)?.Name ?? typeof(T).Name;

            /// <summary>
            /// Creates a result bucket indicating the entity is present in the right collection only.
            /// </summary>
            /// <param name="right">The right entity.</param>
            /// <returns>The result bucket.</returns>
            public static ResultBucket<T> PresentInRight(T right)
               => new ResultBucket<T>
               {
                   Left = default,
                   Right = right,
                   CompareResult = CompareResult.PresentInRightOnly,
               };

            /// <summary>
            /// Creates a result bucket indicating the entity is present in the left collection only.
            /// </summary>
            /// <param name="left">The left entity.</param>
            /// <returns>The result bucket.</returns>
            public static ResultBucket<T> PresentInLeft(T left)
                => new ResultBucket<T>
                {
                    Left = left,
                    Right = default,
                    CompareResult = CompareResult.PresentInLeftOnly,
                };

            /// <summary>
            /// Creates a result bucket indicating the entity is present in both collections but has changed.
            /// </summary>
            /// <param name="left">The left entity.</param>
            /// <param name="right">The right entity.</param>
            /// <returns>The result bucket.</returns>
            public static ResultBucket<T> Changed(T left, T right)
                => new ResultBucket<T>
                {
                    Left = left,
                    Right = right,
                    CompareResult = CompareResult.Changed,
                };

            /// <summary>
            /// Creates a result bucket indicating the entity is present in both collections and is unchanged.
            /// </summary>
            /// <param name="left">The left entity.</param>
            /// <param name="right">The right entity.</param>
            /// <returns>The result bucket.</returns>
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
