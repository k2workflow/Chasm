#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace SourceCode.Chasm.IO.Text.Wire
{
    internal static class CommitWireExtensions
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
            sb.AppendLine($"{_treeId}{model.TreeId.Sha1:N}");

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
            const string author = "unknown";
            var utc = XmlConvert.ToString(model.Utc, XmlDateTimeSerializationMode.Utc);
            sb.AppendLine($"{_author}{utc}"); // TODO: Use compatible datetime format

            // Committer
            const string committer = "unknown";
            utc = XmlConvert.ToString(model.Utc, XmlDateTimeSerializationMode.Utc);
            sb.AppendLine($"{_committer}{utc}"); // TODO: Use compatible datetime format

            // Message
            var msg = sb.AppendLine().AppendLine(model.Message);

            var wire = msg.ToString();
            return wire;
        }

        public static Commit ConvertCommit(this string wire)
        {
            if (string.IsNullOrWhiteSpace(wire)) return default;

            // TreeId
            TreeId treeId;
            var index = wire.IndexOf(_treeId);
            {
                if (index < 0) throw new SerializationException();
                index += _treeId.Length;
                var curr = wire.Substring(index, Sha1.CharLen);
                index += Sha1.CharLen;

                var sha1 = Sha1.Parse(curr);
                treeId = new TreeId(sha1);
            }

            // Parents
            var parents = new List<CommitId>();
            {
                var ix = wire.IndexOf(_parent, index);
                while (ix >= 0)
                {
                    index = ix + _parent.Length;
                    var curr = wire.Substring(index, Sha1.CharLen);
                    index += Sha1.CharLen;

                    var sha1 = Sha1.Parse(curr);
                    parents.Add(new CommitId(sha1));

                    ix = wire.IndexOf(_parent, index);
                }
            }

            // Author
            var authorUtc = ParseAuditLine(_author);

            // Committer
            var committerUtc = ParseAuditLine(_committer);

            // Message
            var message = wire.Substring(index).Trim(new char[] { '\r', '\n' });

            // Done
            var model = new Commit(parents, treeId, authorUtc, message);
            return model;

            // Local functions
            DateTime ParseAuditLine(string name)
            {
                DateTime utc = default;

                var ix = wire.IndexOf(name, index);
                if (ix >= 0)
                {
                    index = ix + name.Length;

                    ix = wire.IndexOf('\n', index);
                    if (ix >= 0)
                    {
                        index = ix + 1;

                        //var foundFirst = false;
                        for (var jx = ix - 1; jx >= 0; jx--)
                        {
                            if (wire[jx] != ' ') continue;

                            //if (foundFirst)
                            {
                                var str = wire.Substring(jx, ix - jx).TrimEnd();
                                utc = XmlConvert.ToDateTime(str, XmlDateTimeSerializationMode.Utc);
                                break;
                            }

                            //foundFirst = true;
                        }
                    }
                }

                return utc;
            }
        }

        #endregion
    }
}
