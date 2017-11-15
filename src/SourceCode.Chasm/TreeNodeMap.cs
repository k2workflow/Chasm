#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{ToString(),nq,ac}")]
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public readonly struct TreeNodeMap : IReadOnlyDictionary<string, TreeNode>, IReadOnlyList<TreeNode>, IEquatable<TreeNodeMap>
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        #region Constants

        /// <summary>
        /// A singleton representing an empty <see cref="TreeNodeMap"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static TreeNodeMap Empty { get; }

        #endregion

        #region Fields

        internal readonly ReadOnlyMemory<TreeNode> _nodes;

        #endregion

        #region Properties

        public int Count => _nodes.Length;

        public TreeNode this[int index]
        {
            get
            {
                if (_nodes.IsEmpty)
                    return Array.Empty<TreeNode>()[index]; // Throw underlying exception

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

        public TreeNodeMap(params TreeNode[] nodes)
        {
            // We choose to coerce empty & null, so de/serialization round-trips with fidelity
            if (nodes == null || nodes.Length == 0)
            {
                _nodes = default; // ie, same as default struct ctor
                return;
            }

            // Sort & de-duplicate
            _nodes = DistinctSort(nodes, false);
        }

        public TreeNodeMap(IEnumerable<TreeNode> nodes)
        {
            // We choose to coerce empty & null, so de/serialization round-trips with fidelity
            if (nodes == null)
            {
                _nodes = default; // ie, same as default struct ctor
                return;
            }

            // Sort & de-duplicate
            _nodes = DistinctSort(nodes);
        }

        public TreeNodeMap(ICollection<TreeNode> nodes)
        {
            // We choose to coerce empty & null, so de/serialization round-trips with fidelity
            if (nodes == null || nodes.Count == 0)
            {
                _nodes = default; // ie, same as default struct ctor
                return;
            }

            // Copy
            var array = new TreeNode[nodes.Count];
            nodes.CopyTo(array, 0);

            // Sort & de-duplicate
            _nodes = DistinctSort(array, true);
        }

        private TreeNodeMap(ReadOnlyMemory<TreeNode> nodes)
        {
            _nodes = nodes;
        }

        #endregion

        #region Methods

        private static ReadOnlyMemory<TreeNode> Merge(TreeNodeMap first, TreeNodeMap second)
        {
            var newArray = new TreeNode[first.Count + second.Count];

            var i = 0;
            var aIndex = 0;
            var bIndex = 0;
            for (; aIndex < first.Count || bIndex < second.Count; i++)
            {
                if (aIndex >= first.Count)
                {
                    newArray[i] = second[bIndex++];
                }
                else if (bIndex >= second.Count)
                {
                    newArray[i] = first[aIndex++];
                }
                else
                {
                    var a = first[aIndex];
                    var b = second[bIndex];
                    var cmp = string.CompareOrdinal(a.Name, b.Name);

                    if (cmp == 0)
                    {
                        newArray[i] = b;
                        ++bIndex;
                        ++aIndex;
                    }
                    else if (cmp < 0)
                    {
                        newArray[i] = a;
                        ++aIndex;
                    }
                    else
                    {
                        newArray[i] = b;
                        ++bIndex;
                    }
                }
            }

            return new ReadOnlyMemory<TreeNode>(newArray, 0, i);
        }

        public TreeNodeMap Merge(TreeNode node)
        {
            if (_nodes.IsEmpty) return new TreeNodeMap(node);

            var index = IndexOf(node.Name);

            var span = _nodes.Span;

            TreeNode[] array;
            if (index >= 0)
            {
                array = new TreeNode[_nodes.Length];
                span.CopyTo(array);
                array[index] = node;
            }
            else
            {
                index = ~index;
                array = new TreeNode[_nodes.Length + 1];

                var j = 0;
                for (var i = 0; i < array.Length; i++)
                    array[i] = i == index ? node : span[j++];
            }

            return new TreeNodeMap(array);
        }

        public TreeNodeMap Delete(Func<string, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (_nodes.Length == 0) return this;

            var copy = new TreeNode[_nodes.Length - 1];
            var span = _nodes.Span;
            var j = 0;
            for (var i = 0; i < _nodes.Length; i++)
            {
                if (!predicate(span[i].Name))
                    copy[j++] = span[i];
            }

            if (j == _nodes.Length)
                return this;

            return new TreeNodeMap(new ReadOnlyMemory<TreeNode>(copy, 0, j));
        }

        public TreeNodeMap Delete(string key)
        {
            if (_nodes.Length == 0) return this;

            var copy = new TreeNode[_nodes.Length - 1];
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
                    found = StringComparer.Ordinal.Equals(span[i].Name, key);
                }
            }

            if (found)
                return new TreeNodeMap(new ReadOnlyMemory<TreeNode>(copy));

            return this;
        }

        public TreeNodeMap Merge(TreeNodeMap nodes)
        {
            if (nodes == default || nodes.Count == 0)
                return this;

            if (_nodes.IsEmpty || _nodes.Length == 0)
                return nodes;

            var set = Merge(this, nodes);

            var tree = new TreeNodeMap(set);
            return tree;
        }

        public TreeNodeMap Merge(ICollection<TreeNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return this;

            if (_nodes.IsEmpty)
                return new TreeNodeMap(nodes);

            var set = Merge(this, new TreeNodeMap(nodes));
            var tree = new TreeNodeMap(set);
            return tree;
        }

        public int IndexOf(string key)
        {
            if (_nodes.IsEmpty || key == null) return -1;

            var l = 0;
            var r = _nodes.Length - 1;
            var i = r / 2;
            var ks = key;

            var span = _nodes.Span;

            while (r >= l)
            {
                var cmp = string.CompareOrdinal(span[i].Name, ks);
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
            value = default;

            if (_nodes.IsEmpty || key == null)
                return false;

            var l = 0;
            var r = _nodes.Length - 1;
            var i = r / 2;
            var ks = key;

            var span = _nodes.Span;

            while (r >= l)
            {
                value = span[i];

                var cmp = string.CompareOrdinal(value.Name, ks);
                if (cmp == 0) return true;

                if (cmp > 0) r = i - 1;
                else l = i + 1;

                i = l + (r - l) / 2;
            }

            value = default;
            return false;
        }

        public bool TryGetValue(string key, NodeKind kind, out TreeNode value)
        {
            value = default;

            if (!TryGetValue(key, out var node))
                return false;

            if (node.Kind != kind)
                return false;

            value = node;
            return true;
        }

        #endregion

        #region Helpers

        private static ReadOnlyMemory<TreeNode> DistinctSort(TreeNode[] array, bool alreadyCopied)
        {
            Debug.Assert(array != null); // Already checked at callsites

            // Optimize for common cases 0, 1, 2, N
            ReadOnlyMemory<TreeNode> result;
            switch (array.Length)
            {
                case 0:
                    return default;

                case 1:
                    // Always copy. The callsite may change the array after creating the TreeNodeMap.
                    result = new TreeNode[1] { array[0] };
                    return result;

                case 2:
                    // If the Name (alone) is duplicated
                    if (StringComparer.Ordinal.Equals(array[0].Name, array[1].Name))
                    {
                        // If it's a complete duplicate, silently skip
                        if (TreeNodeComparer.Default.Equals(array[0], array[1]))
                            result = new TreeNode[1] { array[0] };
                        else
                            throw CreateDuplicateException(array[0]);
                    }
                    // In the wrong order
                    else if (TreeNodeComparer.Default.Compare(array[0], array[1]) > 0)
                    {
                        result = new TreeNode[2] { array[1], array[0] };
                    }
                    else if (alreadyCopied)
                    {
                        result = array;
                    }
                    else
                    {
                        result = new TreeNode[2] { array[0], array[1] };
                    }
                    return result;

                default:

                    // If callsite did not already copy, do so before mutating
                    var nodes = array;
                    if (!alreadyCopied)
                    {
                        nodes = new TreeNode[array.Length];
                        array.CopyTo(nodes, 0);
                    }

                    // Sort: Delegate dispatch faster than interface (https://github.com/dotnet/coreclr/pull/8504)
                    Array.Sort(nodes, TreeNodeComparer.Default.Compare);

                    // Distinct
                    var j = 1;
                    for (var i = 1; i < nodes.Length; i++)
                    {
                        // If the Name (alone) is duplicated
                        if (StringComparer.Ordinal.Equals(nodes[i - 1].Name, nodes[i].Name))
                        {
                            // If it's a complete duplicate, silently skip
                            if (TreeNodeComparer.Default.Equals(nodes[i - 1], nodes[i]))
                                continue;

                            throw CreateDuplicateException(nodes[0]);
                        }
                        nodes[j++] = nodes[i]; // Increment target index if distinct
                    }

                    var span = new ReadOnlyMemory<TreeNode>(nodes, 0, j);
                    return span;
            }

            ArgumentException CreateDuplicateException(TreeNode node)
                => new ArgumentException($"Duplicate {nameof(TreeNode)} arguments passed to {nameof(TreeNodeMap)}: ({node})");
        }

        private static ReadOnlyMemory<TreeNode> DistinctSort(IEnumerable<TreeNode> nodes)
        {
            Debug.Assert(nodes != null); // Already checked at callsites

            if (!System.Linq.Enumerable.Any(nodes)) return default;

            // If callsite did not already copy, do so before mutating
            var array = new List<TreeNode>(nodes).ToArray();

            return DistinctSort(array, true);
        }

        #endregion

        #region IEnumerable

        public IEnumerable<string> Keys
        {
            get
            {
                if (_nodes.IsEmpty)
                    yield break;

                // TODO: Is Span safe under yield?
                var span = _nodes.Span;

                for (var i = 0; i < _nodes.Length; i++)
                    yield return span[i].Name;
            }
        }

        public IEnumerable<TreeNode> Values
        {
            get
            {
                if (_nodes.IsEmpty)
                    yield break;

                // TODO: Is Span safe under yield?
                var span = _nodes.Span;

                for (var i = 0; i < _nodes.Length; i++)
                    yield return span[i];
            }
        }

        public IEnumerator<TreeNode> GetEnumerator() => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<KeyValuePair<string, TreeNode>> IEnumerable<KeyValuePair<string, TreeNode>>.GetEnumerator()
        {
            if (_nodes.IsEmpty)
                yield break;

            // TODO: Is Span safe under yield?
            var span = _nodes.Span;

            for (var i = 0; i < _nodes.Length; i++)
                yield return new KeyValuePair<string, TreeNode>(span[i].Name, span[i]);
        }

        #endregion

        #region IEquatable

        public bool Equals(TreeNodeMap other) => TreeNodeMapComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is TreeNodeMap tree
            && TreeNodeMapComparer.Default.Equals(this, tree);

        public override int GetHashCode() => TreeNodeMapComparer.Default.GetHashCode(this);

        #endregion

        #region Operators

        public static bool operator ==(TreeNodeMap x, TreeNodeMap y) => TreeNodeMapComparer.Default.Equals(x, y);

        public static bool operator !=(TreeNodeMap x, TreeNodeMap y) => !(x == y);

        public override string ToString() => $"{nameof(Count)}: {_nodes.Length}";

        #endregion
    }
}
