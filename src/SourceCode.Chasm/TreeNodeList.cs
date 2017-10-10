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
    public struct TreeNodeList : IReadOnlyDictionary<string, TreeNode>, IReadOnlyList<TreeNode>, IEquatable<TreeNodeList>
    {
        #region Constants

        /// <summary>
        /// A singleton representing an empty <see cref="TreeNodeList"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static TreeNodeList Empty { get; }

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

        public TreeNodeList(params TreeNode[] nodes)
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

        public TreeNodeList(ICollection<TreeNode> nodes)
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

        #endregion

        #region Methods

        private static TreeNode[] Merge(TreeNodeList first, TreeNodeList second)
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

            Array.Resize(ref newArray, i);
            return newArray;
        }

        public TreeNodeList Merge(TreeNode node)
        {
            if (_nodes.IsEmpty) return new TreeNodeList(node);

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

            return new TreeNodeList(array);
        }

        public TreeNodeList Merge(TreeNodeList nodes)
        {
            if (nodes == default || nodes.Count == 0)
                return this;

            if (_nodes.IsEmpty || _nodes.Length == 0)
                return nodes;

            var set = Merge(this, nodes);

            var tree = new TreeNodeList(set);
            return tree;
        }

        public TreeNodeList Merge(ICollection<TreeNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return this;

            if (_nodes.IsEmpty)
                return new TreeNodeList(nodes);

            var set = Merge(this, new TreeNodeList(nodes));
            var tree = new TreeNodeList(set);
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
            switch (array.Length)
            {
                case 1:
                    // Always copy. The callsite may change the array after creating the TreeNodeList.
                    return new TreeNode[1] { array[0] };

                case 2:
                    {
                        // If the Name (alone) is duplicated
                        if (StringComparer.Ordinal.Equals(array[0].Name, array[1].Name))
                        {
                            // If it's a complete duplicate, silently skip
                            if (TreeNodeComparer.Default.Equals(array[0], array[1]))
                                return new TreeNode[1] { array[0] };

                            // Else throw
                            throw CreateDuplicateException(array[0]);
                        }

                        // Sort
                        if (TreeNodeComparer.Default.Compare(array[0], array[1]) < 0)
                            return new TreeNode[2] { array[0], array[1] };

                        return new TreeNode[2] { array[1], array[0] };
                    }

                default:
                    {
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

                                // Else throw
                                throw CreateDuplicateException(nodes[0]);
                            }

                            nodes[j++] = nodes[i]; // Increment target index iff distinct
                        }

                        // Assign
                        var span = new ReadOnlyMemory<TreeNode>(nodes, 0, j);
                        return span;
                    }
            }

            // Local functions
            ArgumentException CreateDuplicateException(TreeNode node)
                => new ArgumentException($"Duplicate {nameof(TreeNode)} arguments passed to {nameof(TreeNodeList)}: ({node})");
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

        public bool Equals(TreeNodeList other) => TreeNodeListComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is TreeNodeList tree
            && TreeNodeListComparer.Default.Equals(this, tree);

        public override int GetHashCode() => TreeNodeListComparer.Default.GetHashCode(this);

        #endregion

        #region Operators

        public static bool operator ==(TreeNodeList x, TreeNodeList y) => TreeNodeListComparer.Default.Equals(x, y);

        public static bool operator !=(TreeNodeList x, TreeNodeList y) => !(x == y);

        public override string ToString() => $"{nameof(Count)}: {_nodes.Length}";

        #endregion
    }
}
