using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
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

        internal readonly TreeNode[] _nodes; // May be null due to default ctor

        #endregion

        #region Properties

        public TreeNode this[int index]
        {
            get
            {
                if (_nodes == null)
                    return Array.Empty<TreeNode>()[index]; // Throw underlying exception

                return _nodes[index];
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

        public int Count => _nodes?.Length ?? 0;

        #endregion

        #region Constructors

        public TreeNodeList(params TreeNode[] nodes)
        {
            // Coerce null to null
            if (nodes == null || nodes.Length == 0)
            {
                // We choose to coerce empty & null to null, so that serialization round-trips properly.
                // (May be null due to default ctor)
                _nodes = null;
                return;
            }

            // Optimize for common cases 0, 1, 2, N
            switch (nodes.Length)
            {
                case 1:
                    _nodes = new TreeNode[1] { nodes[0] };
                    return;

                case 2:
                    {
                        // Throw if the Name is duplicated
                        if (StringComparer.Ordinal.Equals(nodes[0].Name, nodes[1].Name))
                            throw CreateDuplicateException(nodes[0]);

                        // Else sort & assign
                        var cmp = TreeNodeComparer.Default.Compare(nodes[0], nodes[1]);
                        if (cmp < 0)
                            _nodes = new TreeNode[2] { nodes[0], nodes[1] };
                        else
                            _nodes = new TreeNode[2] { nodes[1], nodes[0] };
                    }
                    return;

                default:
                    {
                        // Assume it's already sorted
                        var sorted = true;
                        var array = new TreeNode[nodes.Length];
                        var uniqueName = new HashSet<string>(nodes.Length, StringComparer.Ordinal);

                        // Copy
                        for (var i = 0; i < array.Length; i++)
                        {
                            array[i] = nodes[i];

                            if (!uniqueName.Add(array[i].Name))
                                throw CreateDuplicateException(array[i]);

                            // If it's empirically still sorted
                            if (sorted && i > 0)
                            {
                                // Streaming assertion
                                var cmp = TreeNodeComparer.Default.Compare(array[i - 1], array[i]);
                                if (cmp > 0)
                                    sorted = false;
                            }
                        }

                        // Sort iff necessary
                        if (!sorted)
                        {
                            // Delegate dispatch faster than interface dispatch (https://github.com/dotnet/coreclr/pull/8504)
                            Array.Sort(array, TreeNodeComparer.Default.Compare);
                        }

                        // Assign (sorted by Name)
                        _nodes = array;
                    }
                    return;
            }
        }

        public TreeNodeList(ICollection<TreeNode> nodes)
        {
            // Coerce null to null
            if (nodes == null || nodes.Count == 0)
            {
                // We choose to coerce empty & null to null, so that serialization round-trips properly.
                // (May be null due to default ctor)
                _nodes = null;
                return;
            }

            // Optimize for common cases 0, 1, 2, N
            switch (nodes.Count)
            {
                case 1:
                    using (var enm = nodes.GetEnumerator())
                    {
                        enm.MoveNext();
                        var node0 = enm.Current;

                        _nodes = new TreeNode[1] { node0 };
                    }
                    return;

                case 2:
                    using (var enm = nodes.GetEnumerator())
                    {
                        enm.MoveNext();
                        var node0 = enm.Current;

                        enm.MoveNext();
                        var node1 = enm.Current;

                        // Throw if the Name is duplicated
                        if (StringComparer.Ordinal.Equals(node0.Name, node1.Name))
                            throw CreateDuplicateException(node0);

                        // Else sort & assign
                        var cmp = TreeNodeComparer.Default.Compare(node0, node1);
                        if (cmp < 0)
                            _nodes = new TreeNode[2] { node0, node1 };
                        else
                            _nodes = new TreeNode[2] { node1, node0 };
                    }
                    return;

                default:
                    {
                        // Assume it's already sorted
                        var sorted = true;
                        var array = new TreeNode[nodes.Count];
                        var uniqueName = new HashSet<string>(nodes.Count, StringComparer.Ordinal);

                        // Copy
                        var i = 0;
                        foreach (var node in nodes)
                        {
                            array[i] = node;

                            if (!uniqueName.Add(array[i].Name))
                                throw CreateDuplicateException(array[i]);

                            // If it's empirically still sorted
                            if (sorted && i > 0)
                            {
                                // Streaming assertion
                                var cmp = TreeNodeComparer.Default.Compare(array[i - 1], array[i]);
                                if (cmp > 0)
                                    sorted = false;
                            }

                            i += 1;
                        }

                        // Sort iff necessary
                        if (!sorted)
                        {
                            // Delegate dispatch faster than interface dispatch (https://github.com/dotnet/coreclr/pull/8504)
                            Array.Sort(array, TreeNodeComparer.Default.Compare);
                        }

                        // Assign (sorted by Name)
                        _nodes = array;
                    }
                    return;
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

        private static ArgumentException CreateDuplicateException(TreeNode node)
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
            if (_nodes == null || _nodes.Length == 0)
                yield break;

            foreach (var item in _nodes)
                yield return new KeyValuePair<string, TreeNode>(item.Name, item);
        }

        #endregion

        #region IEquatable

        public bool Equals(TreeNodeList other) => TreeNodeListComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is TreeNodeList tnl
            && TreeNodeListComparer.Default.Equals(this, tnl);

        public override int GetHashCode() => TreeNodeListComparer.Default.GetHashCode(this);

        #endregion

        #region Operators

        public static bool operator ==(TreeNodeList x, TreeNodeList y) => TreeNodeListComparer.Default.Equals(x, y);

        public static bool operator !=(TreeNodeList x, TreeNodeList y) => !TreeNodeListComparer.Default.Equals(x, y); // not

        public override string ToString() => $"{nameof(TreeNodeList)}: {_nodes?.Length ?? 0}";

        #endregion
    }
}
