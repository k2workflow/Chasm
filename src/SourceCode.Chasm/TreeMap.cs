#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Buffers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TreePair = System.Collections.Generic.KeyValuePair<string, SourceCode.Chasm.TreeNode>;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{ToString(),nq,ac}")]
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public readonly partial struct TreeMap : IReadOnlyDictionary<string, TreeNode>, IReadOnlyList<TreePair>, IReadOnlyList<TreeNode>, IEquatable<TreeMap>
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        #region Constants

        private static readonly TreeMap _empty;

        /// <summary>
        /// A singleton representing an empty <see cref="TreeMap"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static ref readonly TreeMap Empty => ref _empty;

        #endregion

        #region Fields

        internal readonly ReadOnlyMemory<TreePair> _nodes;

        #endregion

        #region Properties

        public int Count => _nodes.Length;

        TreeNode IReadOnlyList<TreeNode>.this[int index]
            => this[index].Value;

        public TreePair this[int index]
        {
            get
            {
                if (_nodes.Length == 0)
                    return Array.Empty<TreePair>()[index]; // Throw underlying exception

                var span = _nodes.Span;

                return span[index];
            }
        }

        public TreeNode this[string key]
        {
            get
            {
                if (!TryGetValue(key, out var node))
                    throw new KeyNotFoundException(nameof(key));

                return node;
            }
        }

        #endregion

        #region Constructors

        public TreeMap(params TreePair[] nodes)
        {
            // We choose to coerce empty & null, so de/serialization round-trips with fidelity
            if (nodes == null || nodes.Length == 0)
            {
                _nodes = default; // ie, same as default struct ctor
                return;
            }

            // Sort & de-duplicate
            _nodes = DistinctSort(Copy(nodes));
        }

        public TreeMap(in IEnumerable<TreePair> nodes)
        {
            // We choose to coerce empty & null, so de/serialization round-trips with fidelity
            if (nodes == null)
            {
                _nodes = default; // ie, same as default struct ctor
                return;
            }

            var copy = new List<TreePair>(nodes).ToArray();

            // Sort & de-duplicate
            _nodes = DistinctSort(copy);
        }

        public TreeMap(in ICollection<TreePair> nodes)
        {
            // We choose to coerce empty & null, so de/serialization round-trips with fidelity
            if (nodes == null || nodes.Count == 0)
            {
                _nodes = default; // ie, same as default struct ctor
                return;
            }

            // Copy
            var array = new TreePair[nodes.Count];
            nodes.CopyTo(array, 0);

            // Sort & de-duplicate
            _nodes = DistinctSort(array);
        }

        public TreeMap(in ReadOnlyMemory<TreePair> nodes)
        {
            if (nodes.Length == 0)
            {
                _nodes = default;
                return;
            }

            _nodes = DistinctSort(Copy(nodes));
        }

        private TreeMap(bool sentinel, in ReadOnlyMemory<TreePair> nodes)
        {
            _nodes = nodes;
        }

        #endregion

        #region Merge

        public TreeMap Merge(string key, in TreeNode node)
            => Merge(new TreePair(key, node));

        public TreeMap Merge(in TreePair node)
        {
            if (node.Key == null) throw new ArgumentOutOfRangeException(nameof(node));
            if (_nodes.Length == 0) return new TreeMap(node);

            var index = IndexOf(node.Key);

            var span = _nodes.Span;

            TreePair[] array;
            if (index >= 0)
            {
                array = new TreePair[_nodes.Length];
                span.CopyTo(array);
                array[index] = node;
            }
            else
            {
                index = ~index;
                array = new TreePair[_nodes.Length + 1];

                var j = 0;
                for (var i = 0; i < array.Length; i++)
                    array[i] = i == index ? node : span[j++];
            }

            return new TreeMap(array);
        }

        public TreeMap Merge(in TreeMap nodes)
        {
            if (nodes.Count == 0)
                return this;

            if (_nodes.Length == 0)
                return nodes;

            var tree = MergeImpl(this, nodes);
            return tree;
        }

        private static TreeMap MergeImpl(in TreeMap first, in TreeMap second)
        {
            var newArray = new TreePair[first.Count + second.Count];

            var i = 0;
            var firstIndex = 0;
            var secondIndex = 0;
            var firstSpan = first._nodes.Span;
            var secondSpan = second._nodes.Span;
            for (; firstIndex < firstSpan.Length || secondIndex < secondSpan.Length; i++)
            {
                if (firstIndex >= firstSpan.Length)
                {
                    newArray[i] = secondSpan[secondIndex++];
                }
                else if (secondIndex >= secondSpan.Length)
                {
                    newArray[i] = firstSpan[firstIndex++];
                }
                else
                {
                    var a = firstSpan[firstIndex];
                    var b = secondSpan[secondIndex];
                    var cmp = string.CompareOrdinal(a.Key, b.Key);

                    if (cmp == 0)
                    {
                        newArray[i] = b;
                        ++secondIndex;
                        ++firstIndex;
                    }
                    else if (cmp < 0)
                    {
                        newArray[i] = a;
                        ++firstIndex;
                    }
                    else
                    {
                        newArray[i] = b;
                        ++secondIndex;
                    }
                }
            }

            var mem = new ReadOnlyMemory<TreePair>(newArray, 0, i);

            var tree = new TreeMap(true, mem);
            return tree;
        }

        #endregion

        #region Delete

        public TreeMap Delete(in Func<string, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (_nodes.Length == 0) return this;

            var copy = new TreePair[_nodes.Length - 1];
            var span = _nodes.Span;
            var j = 0;
            for (var i = 0; i < _nodes.Length; i++)
            {
                if (!predicate(span[i].Key))
                    copy[j++] = span[i];
            }

            if (j == _nodes.Length)
                return this;

            return new TreeMap(true, new ReadOnlyMemory<TreePair>(copy, 0, j));
        }

        public TreeMap Delete(string key)
        {
            if (_nodes.Length == 0) return this;

            var copy = new TreePair[_nodes.Length - 1];
            var span = _nodes.Span;
            var found = false;
            for (var i = 0; i < _nodes.Length; i++)
            {
                if (found)
                {
                    copy[i - 1] = span[i];
                }
                else
                {
                    if (i < _nodes.Length - 1)
                        copy[i] = span[i];
                    found = StringComparer.Ordinal.Equals(span[i].Key, key);
                }
            }

            if (found)
                return new TreeMap(true, new ReadOnlyMemory<TreePair>(copy));

            return this;
        }

        #endregion

        #region IReadOnlyDictionary

        public int IndexOf(string key)
        {
            if (_nodes.Length == 0 || key == null) return -1;

            var l = 0;
            var r = _nodes.Length - 1;
            var i = r / 2;
            var ks = key;

            var span = _nodes.Span;

            while (r >= l)
            {
                var cmp = string.CompareOrdinal(span[i].Key, ks);
                if (cmp == 0) return i;
                else if (cmp > 0) r = i - 1;
                else l = i + 1;

                i = l + (r - l) / 2;
            }

            return ~i;
        }

        public bool ContainsKey(string key) => IndexOf(key) >= 0;

        public bool TryGetValue(string key, out TreeNode value)
        {
            if (_nodes.Length == 0 || key == null)
            {
                value = default;
                return false;
            }

            var l = 0;
            var r = _nodes.Length - 1;
            var i = r / 2;
            var ks = key;

            var span = _nodes.Span;

            while (r >= l)
            {
                var kvp = span[i];

                var cmp = string.CompareOrdinal(kvp.Key, ks);
                if (cmp == 0)
                {
                    value = kvp.Value;
                    return true;
                }

                if (cmp > 0) r = i - 1;
                else l = i + 1;

                i = l + (r - l) / 2;
            }

            value = default;
            return false;
        }

        #endregion

        #region Helpers

        private static Memory<TreePair> Copy(ReadOnlyMemory<TreePair> memory)
        {
            var copy = new TreePair[memory.Length];
            memory.Span.CopyTo(copy);
            return copy;
        }

        private static ReadOnlyMemory<TreePair> DistinctSort(Memory<TreePair> nodes)
        {
            var array = nodes.Span;

            // Optimize for common cases 0, 1, 2, N
            switch (array.Length)
            {
                case 0:
                    return default;

                case 1:

                    if (array[0].Key == null) throw new ArgumentOutOfRangeException(nameof(nodes));
                    return nodes;

                case 2:

                    // If the Name (alone) is duplicated
                    if (array[0].Key == null || array[1].Key == null)
                    {
                        throw new ArgumentOutOfRangeException(nameof(nodes));
                    }
                    else if (StringComparer.Ordinal.Equals(array[0].Key, array[1].Key))
                    {
                        // If it's a complete duplicate, silently skip
                        if (TreeNodeComparer.Default.Equals(array[0].Value, array[1].Value))
                            return new TreePair[1] { array[0] };
                        else
                            throw CreateDuplicateException(array[0]);
                    }
                    // In the wrong order
                    else if (TreePairComparer.Default.Compare(array[0], array[1]) > 0)
                    {
                        return new TreePair[2] { array[1], array[0] };
                    }
                    else
                    {
                        return nodes;
                    }

                default:
                    
                    nodes.Span.Sort(TreePairComparer.Default.Compare);

                    // Null gets sorted to the front.
                    if (array[0].Key == null)
                        throw new ArgumentOutOfRangeException(nameof(nodes));

                    // Distinct
                    var j = 1;
                    for (var i = 1; i < array.Length; i++)
                    {
                        // If the Name (alone) is duplicated
                        if (StringComparer.Ordinal.Equals(array[i - 1].Key, array[i].Key))
                        {
                            // If it's a complete duplicate, silently skip
                            if (TreeNodeComparer.Default.Equals(array[i - 1].Value, array[i].Value))
                                continue;

                            throw CreateDuplicateException(array[i]);
                        }
                        array[j++] = array[i]; // Increment target index if distinct
                    }

                    return nodes.Slice(0, j);
            }

            ArgumentException CreateDuplicateException(TreePair node)
                => new ArgumentException($"Duplicate {nameof(TreeNode)} arguments passed to {nameof(TreeMap)}: ({node.Key},{node.Value})");
        }
        
        #endregion

        #region IEnumerable

        public IEnumerable<string> Keys
        {
            get
            {
                if (_nodes.Length == 0)
                    yield break;

                // Span unsafe under yield, so cannot cache it before iterator
                for (var i = 0; i < _nodes.Length; i++)
                    yield return _nodes.Span[i].Key;
            }
        }

        public IEnumerable<TreeNode> Values
        {
            get
            {
                if (_nodes.Length == 0)
                    yield break;

                // Span unsafe under yield, so cannot cache it before iterator
                for (var i = 0; i < _nodes.Length; i++)
                    yield return _nodes.Span[i].Value;
            }
        }

        IEnumerator<TreeNode> IEnumerable<TreeNode>.GetEnumerator() => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<KeyValuePair<string, TreeNode>> GetEnumerator()
        {
            if (_nodes.Length == 0)
                yield break;

            for (var i = 0; i < _nodes.Length; i++)
            {
                // Span unsafe under yield, so cannot cache it before iterator
                var span = _nodes.Span[i];
                yield return span;
            }
        }

        #endregion

        #region IEquatable

        public bool Equals(TreeMap other) => TreeMapComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is TreeMap tree
            && TreeMapComparer.Default.Equals(this, tree);

        public override int GetHashCode() => TreeMapComparer.Default.GetHashCode(this);

        #endregion

        #region Operators

        public static bool operator ==(TreeMap x, TreeMap y) => TreeMapComparer.Default.Equals(x, y);

        public static bool operator !=(TreeMap x, TreeMap y) => !(x == y);

        public override string ToString() => $"{nameof(Count)}: {_nodes.Length}";

        #endregion
    }
}
