using SourceCode.Clay.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public struct TreeNodeList : IReadOnlyDictionary<string, TreeNode>, IReadOnlyList<TreeNode>, IEquatable<TreeNodeList>
    {
        #region Constants

        public static TreeNodeList Empty { get; }

        #endregion

        #region Fields

        private readonly TreeNode[] _nodes;

        #endregion

        #region Properties

        public TreeNode this[int index]
        {
            get
            {
                if (_nodes == null) throw new ArgumentOutOfRangeException(nameof(index));
                return _nodes[index];
            }
        }

        public TreeNode this[string key]
        {
            get
            {
                if (!TryGetValue(key, out var node)) throw new KeyNotFoundException(nameof(key));
                return node;
            }
        }

        public int Count => _nodes?.Length ?? 0;

        #endregion

        #region Constructors

        public TreeNodeList(params TreeNode[] nodes)
        {
            if (nodes == null)
            {
                _nodes = null;
                return;
            }

            // Optimize for common cases
            switch (nodes.Length)
            {
                case 0:
                    _nodes = Array.Empty<TreeNode>();
                    break;

                case 1:
                    _nodes = new TreeNode[1] { nodes[0] };
                    break;

                case 2:
                    {
                        // Throw if the Sha1 is duplicated
                        if (nodes[0].Sha1 == nodes[1].Sha1)
                            throw BuildDuplicateException(nodes[0]);

                        // Throw if the Name is duplicated
                        var cmp = StringComparer.Ordinal.Compare(nodes[0].Name, nodes[1].Name);
                        if (cmp == 0)
                            throw BuildDuplicateException(nodes[0]);

                        // Else sort & assign
                        if (cmp < 0)
                            _nodes = new TreeNode[2] { nodes[0], nodes[1] };
                        else
                            _nodes = new TreeNode[2] { nodes[1], nodes[0] };
                    }
                    break;

                default:
                    {
                        // Copy
                        var array = new TreeNode[nodes.Length];
                        for (var i = 0; i < array.Length; i++)
                            array[i] = nodes[i];

                        // Sort (and throw for duplicates)
                        if (!SortByName(ref array, out var duplicate))
                            throw BuildDuplicateException(duplicate);

                        // Assign (sorted by Name)
                        _nodes = array;
                    }
                    break;
            }
        }

        public TreeNodeList(ICollection<TreeNode> nodes)
        {
            if (nodes == null)
            {
                _nodes = null;
                return;
            }

            // Optimize for common cases
            switch (nodes.Count)
            {
                case 0:
                    _nodes = Array.Empty<TreeNode>();
                    break;

                case 1:
                    _nodes = new TreeNode[1] { nodes.GetEnumerator().Current };
                    break;

                default:
                    {
                        // Copy
                        var i = 0;
                        var array = new TreeNode[nodes.Count];
                        foreach (var node in nodes)
                            array[i++] = node;

                        // Sort (and throw for duplicates)
                        if (!SortByName(ref array, out var duplicate))
                            throw BuildDuplicateException(duplicate);

                        // Assign (sorted by Name)
                        _nodes = array;
                    }
                    break;
            }
        }

        #endregion

        #region Methods

        public TreeNodeList Merge(TreeNode node)
        {
            if (_nodes == null) return new TreeNodeList(node);

            var index = IndexOf(node.Name);

            TreeNode[] arr;
            if (index >= 0)
            {
                arr = new TreeNode[_nodes.Length];
                Array.Copy(_nodes, arr, _nodes.Length);
                arr[index] = node;
            }
            else
            {
                index = ~index;
                arr = new TreeNode[_nodes.Length + 1];

                var j = 0;
                for (var i = 0; i < arr.Length; i++)
                    arr[i] = i == index ? node : _nodes[j++];
            }

            return new TreeNodeList(arr);
        }

        public TreeNodeList Merge(TreeNodeList nodes)
        {
            if (nodes == default || nodes.Count == 0)
                return this;

            if (_nodes == null || _nodes.Length == 0)
                return nodes;

            var set = Merge(this, nodes);

            var tree = new TreeNodeList(set);
            return tree;
        }

        public TreeNodeList Merge(ICollection<TreeNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return this;

            if (_nodes == null || _nodes.Length == 0)
                return new TreeNodeList(nodes);

            var set = Merge(this, new TreeNodeList(nodes));
            var tree = new TreeNodeList(set);
            return tree;
        }

        private static TreeNode[] Merge(TreeNodeList first, TreeNodeList second)
        {
            var newArray = new TreeNode[first.Count + second.Count];

            var i = 0;
            var aIndex = 0;
            var bIndex = 0;
            for (; aIndex < first.Count || bIndex < second.Count; i++)
            {
                if (aIndex >= first.Count)
                    newArray[i] = second[bIndex++];
                else if (bIndex >= second.Count)
                    newArray[i] = first[aIndex++];
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

        public int IndexOf(string key)
        {
            if (_nodes == null || key == null) return -1;

            var l = 0;
            var r = _nodes.Length - 1;
            var i = r / 2;
            var ks = key;

            while (r >= l)
            {
                var svc = _nodes[i];

                var cmp = string.CompareOrdinal(svc.Name, ks);
                if (cmp == 0) return i;
                else if (cmp > 0) r = i - 1;
                else l = i + 1;

                i = l + (r - l) / 2;
            }

            return ~i;
        }

        public bool ContainsKey(string key)
            => IndexOf(key) >= 0;

        public bool TryGetValue(string key, out TreeNode value)
        {
            value = default;

            if (_nodes == null || key == null)
                return false;

            var l = 0;
            var r = _nodes.Length - 1;
            var i = r / 2;
            var ks = key;

            while (r >= l)
            {
                value = _nodes[i];

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

        private static bool SortByName(ref TreeNode[] nodes, out TreeNode duplicate)
        {
            duplicate = default;

            // Sort by Sha1 first (as a short-cut for duplicate detection)
            Array.Sort(nodes, Sha1Comparison);

            // Throw if any Sha1 is duplicated
            for (var i = 1; i < nodes.Length; i++)
            {
                if (nodes[i].Sha1 == nodes[i - 1].Sha1)
                {
                    duplicate = nodes[i];
                    return false;
                }
            }

            // Then sort by Name, since that's the final order we need
            Array.Sort(nodes, NameComparison);

            // Throw if any Name is duplicated
            for (var i = 1; i < nodes.Length; i++)
            {
                if (StringComparer.Ordinal.Compare(nodes[i].Name, nodes[i - 1].Name) == 0)
                {
                    duplicate = nodes[i];
                    return false;
                }
            }

            return true;

            // Delegate dispatch faster than interface dispatch (https://github.com/dotnet/coreclr/pull/8504)
            int Sha1Comparison(TreeNode x, TreeNode y)
                => Sha1.DefaultComparer.Compare(x.Sha1, y.Sha1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int NameComparison(TreeNode x, TreeNode y)
            // Delegate dispatch faster than interface dispatch (https://github.com/dotnet/coreclr/pull/8504)
            => string.CompareOrdinal(x.Name, y.Name);

        private static ArgumentException BuildDuplicateException(TreeNode node)
            => new ArgumentException($"Duplicate {nameof(TreeNode)} arguments passed to {nameof(TreeNodeList)}: ({node})");

        #endregion

        #region IEnumerable

        public IEnumerable<string> Keys
        {
            get
            {
                if (_nodes == null || _nodes.Length == 0)
                    yield break;

                foreach (var node in _nodes)
                    yield return node.Name;
            }
        }

        public IEnumerable<TreeNode> Values
        {
            get
            {
                if (_nodes == null || _nodes.Length == 0)
                    yield break;

                foreach (var node in _nodes)
                    yield return node;
            }
        }

        public IEnumerator<TreeNode> GetEnumerator() => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<KeyValuePair<string, TreeNode>> IEnumerable<KeyValuePair<string, TreeNode>>.GetEnumerator()
        {
            if (_nodes == null) yield break;

            foreach (var item in _nodes)
                yield return new KeyValuePair<string, TreeNode>(item.Name, item);
        }

        #endregion

        #region IEquatable

        public override int GetHashCode()
            => _nodes?.Length ?? 19;

        public override bool Equals(object obj)
            => obj is TreeNodeList tnl
            && Equals(tnl);

        public bool Equals(TreeNodeList other)
            => _nodes.NullableEquals(other._nodes, true);

        public static bool operator ==(TreeNodeList left, TreeNodeList right) => left.Equals(right);

        public static bool operator !=(TreeNodeList left, TreeNodeList right) => !left.Equals(right);

        #endregion
    }
}
