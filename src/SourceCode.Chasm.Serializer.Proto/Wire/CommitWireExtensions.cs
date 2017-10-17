#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class CommitWireExtensions
    {
        #region Methods

        public static CommitWire Convert(this Commit model)
        {
            if (model == Commit.Empty)
                return new CommitWire() { Message = string.Empty };

            var wire = new CommitWire
            {
                // TreeId
                TreeId = model.TreeId.Sha1.Convert()
            };

            // Parents
            switch (model.Parents.Count)
            {
                case 0:
                    break;

                case 1:
                    {
                        var sha1 = model.Parents[0].Sha1.Convert();
                        wire.Parents.Add(sha1);
                    }
                    break;

                default:
                    {
                        foreach (var parent in model.Parents)
                        {
                            var sha1 = parent.Sha1.Convert();
                            wire.Parents.Add(sha1);
                        }
                    }
                    break;
            }

            // Author
            wire.Author = model.Author.Convert();

            // Committer
            wire.Committer = model.Committer.Convert();

            // Message
            wire.Message = model.Message;

            return wire;
        }

        public static Commit Convert(this CommitWire wire)
        {
            if (wire == null) return default;

            // TreeId
            var sha1 = wire.TreeId.Convert();
            var treeId = new TreeId(sha1);

            // Parents
            var parents = Array.Empty<CommitId>();
            if (wire.Parents != null && wire.Parents.Count > 0)
            {
                var i = 0;
                parents = new CommitId[wire.Parents.Count];
                foreach (var parent in wire.Parents)
                {
                    sha1 = parent.Convert();
                    parents[i++] = new CommitId(sha1);
                }
            }

            // Author
            var author = Audit.Empty;
            if (wire.Author != null)
            {
                author = wire.Author.Convert();
            }

            // Committer
            var committer = Audit.Empty;
            if (wire.Committer != null)
            {
                committer = wire.Committer.Convert();
            }

            var model = new Commit(parents, treeId, author, committer, wire.Message);
            return model;
        }

        #endregion
    }
}
