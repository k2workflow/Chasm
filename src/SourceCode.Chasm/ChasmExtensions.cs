#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using TreePair = System.Collections.Generic.KeyValuePair<string, SourceCode.Chasm.TreeNode>;

namespace SourceCode.Chasm
{
    public static class ChasmExtensions
    {
        #region TreeNode

        public static bool TryCreateTreeMapId(in this TreeNode treeNode, out TreeMapId value)
        {
            if (treeNode.Kind == NodeKind.Map)
            {
                value = new TreeMapId(treeNode.Sha1);
                return true;
            }
            value = default;
            return false;
        }

        public static bool TryCreateBlobId(in this TreeNode treeNode, out BlobId value)
        {
            if (treeNode.Kind == NodeKind.Blob)
            {
                value = new BlobId(treeNode.Sha1);
                return true;
            }
            value = default;
            return false;
        }

        public static TreeMapId CreateTreeMapId(in this TreeNode treeNode)
        {
            if (!TryCreateTreeMapId(treeNode, out var result))
                throw new InvalidOperationException("Tree node is not a tree map ID.");
            return result;
        }

        public static BlobId CreateBlobId(in this TreeNode treeNode)
        {
            if (!TryCreateBlobId(treeNode, out var result))
                throw new InvalidOperationException("Tree node is not a blob ID.");
            return result;
        }

        public static bool TryCreateTreeMapId(in this TreeNode? treeNode, out TreeMapId value)
        {
            if (treeNode.HasValue && treeNode.Value.Kind == NodeKind.Map)
            {
                value = new TreeMapId(treeNode.Value.Sha1);
                return true;
            }
            value = default;
            return false;
        }

        public static bool TryCreateBlobId(in this TreeNode? treeNode, out BlobId blobId)
        {
            if (treeNode.HasValue && treeNode.Value.Kind == NodeKind.Blob)
            {
                blobId = new BlobId(treeNode.Value.Sha1);
                return true;
            }
            blobId = default;
            return false;
        }

        public static TreeMapId? CreateTreeMapId(in this TreeNode? treeNode)
        {
            if (!treeNode.HasValue) return default;
            if (!TryCreateTreeMapId(treeNode.Value, out var result))
                throw new InvalidOperationException("Tree node is not a tree map ID.");
            return result;
        }

        public static BlobId? CreateBlobId(in this TreeNode? treeNode)
        {
            if (!treeNode.HasValue) return default;
            if (!TryCreateBlobId(treeNode.Value, out var result))
                throw new InvalidOperationException("Tree node is not a blob ID.");
            return result;
        }

        #endregion

        #region TreeMap

        public static TreeMap? Merge(in this TreeMap? first, in TreeMap? second)
        {
            if (!first.HasValue) return second;
            if (!second.HasValue) return first;
            return first.Value.Merge(second.Value);
        }

        public static TreeMap Merge<T>(in this TreeMap first, in T second)
            where T : IEnumerable<TreePair>
        {
            if (second == null) return first;

            // Collection could be slightly faster because of CopyTo.
            var treeMap = second is ICollection<TreePair> collection
                ? new TreeMap(collection)
                : new TreeMap(second);

            return first.Merge(treeMap);
        }

        public static TreeMap? Merge<T>(in this TreeMap? first, in T second)
            where T : IEnumerable<TreePair>
        {
            if (second == null) return first;

            var treeMap = second is ICollection<TreePair> collection
                ? new TreeMap(collection)
                : new TreeMap(second);

            if (!first.HasValue) return treeMap;
            return first.Merge(treeMap);
        }

        public static bool TryGetBlobId(in this TreeMap map, string key, out BlobId value)
            => TryGetValue(map, key, out var node) & node.TryCreateBlobId(out value);

        public static bool TryGetTreeMapId(in this TreeMap map, string key, out TreeMapId value)
            => TryGetValue(map, key, out var node) & node.TryCreateTreeMapId(out value);

        public static bool TryGetValue(in this TreeMap map, string key, NodeKind nodeKind, out TreeNode value)
        {
            if (!map.TryGetValue(key, out value)) return false;
            if (value.Kind == nodeKind) return true;
            value = default;
            return false;
        }

        public static bool TryGetBlobId(in this TreeMap? map, string key, out BlobId value)
            => TryGetValue(map, key, out var node) & node.TryCreateBlobId(out value);

        public static bool TryGetTreeMapId(in this TreeMap? map, string key, out TreeMapId value)
            => TryGetValue(map, key, out var node) & node.TryCreateTreeMapId(out value);

        public static bool TryGetValue(in this TreeMap? map, string key, out TreeNode value)
        {
            if (!map.HasValue)
            {
                value = default;
                return false;
            }
            return map.Value.TryGetValue(key, out value);
        }

        public static bool TryGetValue(in this TreeMap? map, string key, NodeKind nodeKind, out TreeNode value)
        {
            if (!map.HasValue)
            {
                value = default;
                return false;
            }
            return TryGetValue(map.Value, key, nodeKind, out value);
        }

        #endregion
    }
}
