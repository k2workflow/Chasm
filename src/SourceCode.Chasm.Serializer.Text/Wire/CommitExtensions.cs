#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace SourceCode.Chasm.IO.Text.Wire
{
    internal static class CommitExtensions
    {
        #region Constants

        private const string _treeId = "tree ";
        private const string _parent = "parent ";
        private const string _author = "author ";
        private const string _committer = "committer ";

        #endregion

        #region Methods

        public static string Convert(this Commit model)
        {
            var sb = new StringBuilder();

            // Tree
            if (model.TreeId != null)
                sb.AppendLine($"{_treeId}{model.TreeId.Value.Sha1:N}");

            // Parents
            if (model.Parents != null)
            {
                switch (model.Parents.Count)
                {
                    case 0:
                        break;

                    case 1:
                        sb.AppendLine($"{_parent}{model.Parents[0].Sha1:N}");
                        break;

                    default:
                        {
                            for (var i = 0; i < model.Parents.Count; i++)
                                sb.AppendLine($"{_parent}{model.Parents[i].Sha1:N}");
                        }
                        break;
                }
            }

            // Author
            if (model.Author != Audit.Empty)
            {
                var author = model.Author.Convert();
                sb.AppendLine($"{_author}{author}");
            }

            // Committer
            if (model.Committer != Audit.Empty)
            {
                var committer = model.Committer.Convert();
                sb.AppendLine($"{_committer}{committer}");
            }

            // Message
            sb.AppendLine().AppendLine(model.Message);

            var wire = sb.ToString();
            return wire;
        }

        public static Commit ConvertCommit(this string wire)
        {
            if (string.IsNullOrEmpty(wire)) return default;

            // TreeId
            TreeId? treeId = default;
            var index = wire.IndexOf(_treeId, StringComparison.Ordinal);
            {
                if (index == 0)
                {
                    index += _treeId.Length;
                    var curr = wire.Substring(index, Sha1.CharLen);
                    index += Sha1.CharLen;

                    var sha1 = Sha1.Parse(curr);
                    treeId = new TreeId(sha1);
                }
                else
                {
                    index = 0;
                }
            }

            // Parents
            var parents = new List<CommitId>();
            {
                var ix = wire.IndexOf(_parent, index, StringComparison.Ordinal);
                while (ix >= 0)
                {
                    index = ix + _parent.Length;
                    var curr = wire.Substring(index, Sha1.CharLen);
                    index += Sha1.CharLen;

                    var sha1 = Sha1.Parse(curr);
                    parents.Add(new CommitId(sha1));

                    ix = wire.IndexOf(_parent, index, StringComparison.Ordinal);
                }
            }

            // Author
            Audit author = default;
            {
                var ix = wire.IndexOf(_author, index, StringComparison.Ordinal);
                if (ix >= 0)
                {
                    index = ix + _author.Length;

                    ix = wire.IndexOf('\n', index);
                    if (ix >= 0)
                    {
                        var str = wire.Substring(index, ix - index);
                        index = ix;

                        author = str.ConvertAudit();
                    }
                }
            }

            // Committer
            Audit committer = default;
            {
                var ix = wire.IndexOf(_committer, index, StringComparison.Ordinal);
                if (ix >= 0)
                {
                    index = ix + _committer.Length;

                    ix = wire.IndexOf('\n', index);
                    if (ix >= 0)
                    {
                        var str = wire.Substring(index, ix - index); ;
                        index = ix;

                        committer = str.ConvertAudit();
                    }
                }
            }

            // Message
            var message = wire.Substring(index).Trim(new char[] { '\r', '\n' });

            // Done
            var model = new Commit(parents, treeId, author, committer, message);
            return model;
        }

        #endregion
    }
}
