using SourceCode.Clay.Collections.Generic;
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
            }
            else if (nodes.Length == 0)
            {
                _nodes = Array.Empty<TreeNode>();
            }
            else
            {
                var array = new TreeNode[nodes.Length];

                for (var i = 0; i < array.Length; i++)
                    array[i] = nodes[i];

                Array.Sort(array, (x, y) => StringComparer.Ordinal.Compare(x.Name, y.Name));

                _nodes = array;
            }
        }

        public TreeNodeList(ICollection<TreeNode> nodes)
        {
            if (nodes == null)
            {
                _nodes = null;
            }
            else if (nodes.Count == 0)
            {
                _nodes = Array.Empty<TreeNode>();
            }
            else
            {
                var array = new TreeNode[nodes.Count];
                nodes.CopyTo(array, 0);
                Array.Sort(array, (x, y) => StringComparer.Ordinal.Compare(x.Name, y.Name));
                _nodes = array;
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
                    var cmp = StringComparer.Ordinal.Compare(a.Name, b.Name);

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

                var cmp = StringComparer.Ordinal.Compare(svc.Name, ks);
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

                var cmp = StringComparer.Ordinal.Compare(value.Name, ks);
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
