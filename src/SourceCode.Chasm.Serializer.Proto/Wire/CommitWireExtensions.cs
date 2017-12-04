#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Google.Protobuf.WellKnownTypes;
using System;

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class CommitWireExtensions
    {
        #region Methods

        public static CommitWire Convert(this Commit model)
        {
            var wire = new CommitWire
            {
                // TreeId
                TreeId = model.TreeId.Convert()
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
            wire.Message = new StringValue { Value = model.Message };

            // TenantId
            wire.TenantId = new StringValue { Value = model.TenantId };

            return wire;
        }

        public static Commit Convert(this CommitWire wire)
        {
            if (wire == null) return default;

            // TreeId
            var treeId = wire.TreeId.ConvertTree();

            // Parents
            var parents = Array.Empty<CommitId>();
            if (wire.Parents != null && wire.Parents.Count > 0)
            {
                var i = 0;
                parents = new CommitId[wire.Parents.Count];
                foreach (var parent in wire.Parents)
                {
                    // Parents always have values as you simply
                    // don't include nulls in the list
                    parents[i++] = parent.ConvertCommit().Value;
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

            // Message
            var message = wire.Message?.Value;

            // TenantId
            var tenantId = wire.TenantId?.Value;

            var model = new Commit(parents, treeId, author, committer, message, tenantId);
            return model;
        }

        #endregion
    }
}
