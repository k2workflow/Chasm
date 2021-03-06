using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{ToString(),nq,ac}")]
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public readonly struct TreeNodeMap : IReadOnlyDictionary<string, TreeNode>, IReadOnlyList<TreeNode>, IEquatable<TreeNodeMap>
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        private static readonly TreeNodeMap s_empty;

        /// <summary>
        /// A singleton representing an empty <see cref="TreeNodeMap"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static ref readonly TreeNodeMap Empty => ref s_empty;

        internal readonly ReadOnlyMemory<TreeNode> Nodes;

        public int Count => Nodes.Length;

        public TreeNode this[int index]
        {
            get
            {
                if (Nodes.Length == 0)
                    return Array.Empty<TreeNode>()[index]; // Throw underlying exception

                ReadOnlySpan<TreeNode> span = Nodes.Span;

                return span[index];
            }
        }

        public TreeNode this[string key]
            => GetNode(key);

        public TreeNodeMap(params TreeNode[] nodes)
        {
            // We choose to coerce empty & null, so de/serialization round-trips with fidelity
            if (nodes == null || nodes.Length == 0)
            {
                Nodes = default; // ie, same as default struct ctor
                return;
            }

            // Sort & de-duplicate
            Nodes = DistinctSort(nodes, false);
        }

        public TreeNodeMap(in IEnumerable<TreeNode> nodes)
        {
            // We choose to coerce empty & null, so de/serialization round-trips with fidelity
            if (nodes == null)
            {
                Nodes = default; // ie, same as default struct ctor
                return;
            }

            // Sort & de-duplicate
            Nodes = DistinctSort(nodes);
        }

        public TreeNodeMap(in ICollection<TreeNode> nodes)
        {
            // We choose to coerce empty & null, so de/serialization round-trips with fidelity
            if (nodes == null || nodes.Count == 0)
            {
                Nodes = default; // ie, same as default struct ctor
                return;
            }

            // Copy
            var array = new TreeNode[nodes.Count];
            nodes.CopyTo(array, 0);

            // Sort & de-duplicate
            Nodes = DistinctSort(array, true);
        }

        private TreeNodeMap(in ReadOnlyMemory<TreeNode> nodes)
        {
            Nodes = nodes;
        }

        #region Merge

        public TreeNodeMap Add(in TreeNode node)
        {
            if (Nodes.Length == 0) return new TreeNodeMap(node);

            int index = IndexOf(node.Name);

            ReadOnlySpan<TreeNode> span = Nodes.Span;

            TreeNode[] array;
            if (index >= 0)
            {
                array = new TreeNode[Nodes.Length];
                span.CopyTo(array);
                array[index] = node;
            }
            else
            {
                index = ~index;
                array = new TreeNode[Nodes.Length + 1];

                int j = 0;
                for (int i = 0; i < array.Length; i++)
                    array[i] = i == index ? node : span[j++];
            }

            return new TreeNodeMap(array);
        }

        public TreeNodeMap Merge(in TreeNodeMap nodes)
        {
            if (nodes.Count == 0)
                return this;

            if (Nodes.Length == 0)
                return nodes;

            TreeNodeMap tree = MergeImpl(this, nodes);
            return tree;
        }

        public TreeNodeMap Merge(in ICollection<TreeNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return this;

            if (Nodes.Length == 0)
                return new TreeNodeMap(nodes);

            TreeNodeMap tree = MergeImpl(this, new TreeNodeMap(nodes));
            return tree;
        }

        private static TreeNodeMap MergeImpl(in TreeNodeMap first, in TreeNodeMap second)
        {
            var newArray = new TreeNode[first.Count + second.Count];

            int i = 0;
            int aIndex = 0;
            int bIndex = 0;
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
                    TreeNode a = first[aIndex];
                    TreeNode b = second[bIndex];
                    int cmp = string.CompareOrdinal(a.Name, b.Name);

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

            var mem = new ReadOnlyMemory<TreeNode>(newArray, 0, i);

            var tree = new TreeNodeMap(mem);
            return tree;
        }

        #endregion

        #region Delete

        public TreeNodeMap Delete(in Func<string, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (Nodes.Length == 0) return this;

            var copy = new TreeNode[Nodes.Length - 1];
            ReadOnlySpan<TreeNode> span = Nodes.Span;
            int j = 0;
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (!predicate(span[i].Name))
                    copy[j++] = span[i];
            }

            if (j == Nodes.Length)
                return this;

            return new TreeNodeMap(new ReadOnlyMemory<TreeNode>(copy, 0, j));
        }

        public TreeNodeMap Delete(string key)
        {
            if (Nodes.Length == 0) return this;

            var copy = new TreeNode[Nodes.Length - 1];
            ReadOnlySpan<TreeNode> span = Nodes.Span;
            bool found = false;
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (found)
                {
                    copy[i - 1] = span[i];
                }
                else
                {
                    if (i < Nodes.Length - 1)
                        copy[i] = span[i];
                    found = StringComparer.Ordinal.Equals(span[i].Name, key);
                }
            }

            if (found)
                return new TreeNodeMap(new ReadOnlyMemory<TreeNode>(copy));

            return this;
        }

        #endregion

        #region IReadOnlyDictionary

        public int IndexOf(string key)
        {
            if (Nodes.Length == 0 || key == null) return -1;

            int l = 0;
            int r = Nodes.Length - 1;
            int i = r / 2;
            string ks = key;

            ReadOnlySpan<TreeNode> span = Nodes.Span;

            while (r >= l)
            {
                int cmp = string.CompareOrdinal(span[i].Name, ks);
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

            if (Nodes.Length == 0 || key == null)
                return false;

            int l = 0;
            int r = Nodes.Length - 1;
            int i = r / 2;
            string ks = key;

            ReadOnlySpan<TreeNode> span = Nodes.Span;

            while (r >= l)
            {
                value = span[i];

                int cmp = string.CompareOrdinal(value.Name, ks);
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

            if (!TryGetValue(key, out TreeNode node))
                return false;

            if (node.Kind != kind)
                return false;

            value = node;
            return true;
        }

        #endregion

        #region Helpers

        private TreeNode GetNode(string key)
        {
            if (!TryGetValue(key, out TreeNode node))
                throw new KeyNotFoundException(nameof(key));

            return node;
        }

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
                    TreeNode[] nodes = array;
                    if (!alreadyCopied)
                    {
                        nodes = new TreeNode[array.Length];
                        array.CopyTo(nodes, 0);
                    }

                    // Sort: Delegate dispatch faster than interface (https://github.com/dotnet/coreclr/pull/8504)
                    Array.Sort(nodes, TreeNodeComparer.Default.Compare);

                    // Distinct
                    int j = 1;
                    for (int i = 1; i < nodes.Length; i++)
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
            TreeNode[] array = new List<TreeNode>(nodes).ToArray();

            return DistinctSort(array, true);
        }

        #endregion

        #region IEnumerable

        public IEnumerable<string> Keys
        {
            get
            {
                if (Nodes.Length == 0)
                    yield break;

                // Span unsafe under yield, so cannot cache it before iterator
                for (int i = 0; i < Nodes.Length; i++)
                    yield return Nodes.Span[i].Name;
            }
        }

        public IEnumerable<TreeNode> Values
        {
            get
            {
                if (Nodes.Length == 0)
                    yield break;

                // Span unsafe under yield, so cannot cache it before iterator
                for (int i = 0; i < Nodes.Length; i++)
                    yield return Nodes.Span[i];
            }
        }

        public IEnumerator<TreeNode> GetEnumerator() => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<KeyValuePair<string, TreeNode>> IEnumerable<KeyValuePair<string, TreeNode>>.GetEnumerator()
        {
            if (Nodes.Length == 0)
                yield break;

            for (int i = 0; i < Nodes.Length; i++)
            {
                // Span unsafe under yield, so cannot cache it before iterator
                TreeNode span = Nodes.Span[i];
                yield return new KeyValuePair<string, TreeNode>(span.Name, span);
            }
        }

        #endregion

        public override string ToString()
            => $"{nameof(Count)}: {Nodes.Length}";

        public bool Equals(TreeNodeMap other)
            => Nodes.MemoryEquals(other.Nodes);

        public override bool Equals(object obj)
            => obj is TreeNodeMap other
            && Equals(other);

        public override int GetHashCode()
#if !NETSTANDARD2_0
            => HashCode.Combine(obj._nodes.Length);
#else
            => Nodes.Length;
#endif

        public static bool operator ==(TreeNodeMap x, TreeNodeMap y) => x.Nodes.Equals(y.Nodes);

        public static bool operator !=(TreeNodeMap x, TreeNodeMap y) => !(x == y);

        public static TreeNodeMap operator +(TreeNodeMap x, TreeNode y) => x.Add(y);
    }
}
