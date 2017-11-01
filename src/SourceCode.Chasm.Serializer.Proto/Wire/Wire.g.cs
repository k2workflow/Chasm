// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: Wire.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace SourceCode.Chasm.IO.Proto.Wire {

  /// <summary>Holder for reflection information generated from Wire.proto</summary>
  public static partial class WireReflection {

    #region Descriptor
    /// <summary>File descriptor for Wire.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static WireReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CgpXaXJlLnByb3RvIkQKCFNoYTFXaXJlEgsKA1NldBgBIAEoCBINCgVCbGl0",
            "MBgCIAEoBBINCgVCbGl0MRgDIAEoBBINCgVCbGl0MhgEIAEoDSJUCgxUcmVl",
            "V2lyZU5vZGUSDAoETmFtZRgBIAEoCRIbCgRLaW5kGAIgASgOMg0uTm9kZUtp",
            "bmRXaXJlEhkKBk5vZGVJZBgDIAEoCzIJLlNoYTFXaXJlIigKCFRyZWVXaXJl",
            "EhwKBU5vZGVzGAEgAygLMg0uVHJlZVdpcmVOb2RlIjcKCUF1ZGl0V2lyZRIM",
            "CgROYW1lGAEgASgJEgwKBFRpbWUYAiABKAMSDgoGT2Zmc2V0GAMgASgFIo8B",
            "CgpDb21taXRXaXJlEhoKB1BhcmVudHMYASADKAsyCS5TaGExV2lyZRIZCgZU",
            "cmVlSWQYAiABKAsyCS5TaGExV2lyZRIaCgZBdXRob3IYAyABKAsyCi5BdWRp",
            "dFdpcmUSHQoJQ29tbWl0dGVyGAQgASgLMgouQXVkaXRXaXJlEg8KB01lc3Nh",
            "Z2UYBSABKAkiJQoMQ29tbWl0SWRXaXJlEhUKAklkGAEgASgLMgkuU2hhMVdp",
            "cmUqIgoMTm9kZUtpbmRXaXJlEggKBEJsb2IQABIICgRUcmVlEAFCIaoCHlNv",
            "dXJjZUNvZGUuQ2hhc20uSU8uUHJvdG8uV2lyZWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::SourceCode.Chasm.IO.Proto.Wire.NodeKindWire), }, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire), global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire.Parser, new[]{ "Set", "Blit0", "Blit1", "Blit2" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::SourceCode.Chasm.IO.Proto.Wire.TreeWireNode), global::SourceCode.Chasm.IO.Proto.Wire.TreeWireNode.Parser, new[]{ "Name", "Kind", "NodeId" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::SourceCode.Chasm.IO.Proto.Wire.TreeWire), global::SourceCode.Chasm.IO.Proto.Wire.TreeWire.Parser, new[]{ "Nodes" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::SourceCode.Chasm.IO.Proto.Wire.AuditWire), global::SourceCode.Chasm.IO.Proto.Wire.AuditWire.Parser, new[]{ "Name", "Time", "Offset" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::SourceCode.Chasm.IO.Proto.Wire.CommitWire), global::SourceCode.Chasm.IO.Proto.Wire.CommitWire.Parser, new[]{ "Parents", "TreeId", "Author", "Committer", "Message" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::SourceCode.Chasm.IO.Proto.Wire.CommitIdWire), global::SourceCode.Chasm.IO.Proto.Wire.CommitIdWire.Parser, new[]{ "Id" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  /// <summary>
  /// NodeKind
  /// </summary>
  public enum NodeKindWire {
    /// <summary>
    /// Default
    /// </summary>
    [pbr::OriginalName("Blob")] Blob = 0,
    [pbr::OriginalName("Tree")] Tree = 1,
  }

  #endregion

  #region Messages
  /// <summary>
  /// Sha1
  /// </summary>
  public sealed partial class Sha1Wire : pb::IMessage<Sha1Wire> {
    private static readonly pb::MessageParser<Sha1Wire> _parser = new pb::MessageParser<Sha1Wire>(() => new Sha1Wire());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Sha1Wire> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::SourceCode.Chasm.IO.Proto.Wire.WireReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Sha1Wire() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Sha1Wire(Sha1Wire other) : this() {
      set_ = other.set_;
      blit0_ = other.blit0_;
      blit1_ = other.blit1_;
      blit2_ = other.blit2_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Sha1Wire Clone() {
      return new Sha1Wire(this);
    }

    /// <summary>Field number for the "Set" field.</summary>
    public const int SetFieldNumber = 1;
    private bool set_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Set {
      get { return set_; }
      set {
        set_ = value;
      }
    }

    /// <summary>Field number for the "Blit0" field.</summary>
    public const int Blit0FieldNumber = 2;
    private ulong blit0_;
    /// <summary>
    /// 0..7
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong Blit0 {
      get { return blit0_; }
      set {
        blit0_ = value;
      }
    }

    /// <summary>Field number for the "Blit1" field.</summary>
    public const int Blit1FieldNumber = 3;
    private ulong blit1_;
    /// <summary>
    /// 8..15
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong Blit1 {
      get { return blit1_; }
      set {
        blit1_ = value;
      }
    }

    /// <summary>Field number for the "Blit2" field.</summary>
    public const int Blit2FieldNumber = 4;
    private uint blit2_;
    /// <summary>
    /// 16..19
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint Blit2 {
      get { return blit2_; }
      set {
        blit2_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Sha1Wire);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Sha1Wire other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Set != other.Set) return false;
      if (Blit0 != other.Blit0) return false;
      if (Blit1 != other.Blit1) return false;
      if (Blit2 != other.Blit2) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Set != false) hash ^= Set.GetHashCode();
      if (Blit0 != 0UL) hash ^= Blit0.GetHashCode();
      if (Blit1 != 0UL) hash ^= Blit1.GetHashCode();
      if (Blit2 != 0) hash ^= Blit2.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Set != false) {
        output.WriteRawTag(8);
        output.WriteBool(Set);
      }
      if (Blit0 != 0UL) {
        output.WriteRawTag(16);
        output.WriteUInt64(Blit0);
      }
      if (Blit1 != 0UL) {
        output.WriteRawTag(24);
        output.WriteUInt64(Blit1);
      }
      if (Blit2 != 0) {
        output.WriteRawTag(32);
        output.WriteUInt32(Blit2);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Set != false) {
        size += 1 + 1;
      }
      if (Blit0 != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Blit0);
      }
      if (Blit1 != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Blit1);
      }
      if (Blit2 != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(Blit2);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Sha1Wire other) {
      if (other == null) {
        return;
      }
      if (other.Set != false) {
        Set = other.Set;
      }
      if (other.Blit0 != 0UL) {
        Blit0 = other.Blit0;
      }
      if (other.Blit1 != 0UL) {
        Blit1 = other.Blit1;
      }
      if (other.Blit2 != 0) {
        Blit2 = other.Blit2;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            Set = input.ReadBool();
            break;
          }
          case 16: {
            Blit0 = input.ReadUInt64();
            break;
          }
          case 24: {
            Blit1 = input.ReadUInt64();
            break;
          }
          case 32: {
            Blit2 = input.ReadUInt32();
            break;
          }
        }
      }
    }

  }

  /// <summary>
  /// TreeNode
  /// </summary>
  public sealed partial class TreeWireNode : pb::IMessage<TreeWireNode> {
    private static readonly pb::MessageParser<TreeWireNode> _parser = new pb::MessageParser<TreeWireNode>(() => new TreeWireNode());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TreeWireNode> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::SourceCode.Chasm.IO.Proto.Wire.WireReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TreeWireNode() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TreeWireNode(TreeWireNode other) : this() {
      name_ = other.name_;
      kind_ = other.kind_;
      NodeId = other.nodeId_ != null ? other.NodeId.Clone() : null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TreeWireNode Clone() {
      return new TreeWireNode(this);
    }

    /// <summary>Field number for the "Name" field.</summary>
    public const int NameFieldNumber = 1;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Kind" field.</summary>
    public const int KindFieldNumber = 2;
    private global::SourceCode.Chasm.IO.Proto.Wire.NodeKindWire kind_ = 0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::SourceCode.Chasm.IO.Proto.Wire.NodeKindWire Kind {
      get { return kind_; }
      set {
        kind_ = value;
      }
    }

    /// <summary>Field number for the "NodeId" field.</summary>
    public const int NodeIdFieldNumber = 3;
    private global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire nodeId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire NodeId {
      get { return nodeId_; }
      set {
        nodeId_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TreeWireNode);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TreeWireNode other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Name != other.Name) return false;
      if (Kind != other.Kind) return false;
      if (!object.Equals(NodeId, other.NodeId)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (Kind != 0) hash ^= Kind.GetHashCode();
      if (nodeId_ != null) hash ^= NodeId.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Name.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Name);
      }
      if (Kind != 0) {
        output.WriteRawTag(16);
        output.WriteEnum((int) Kind);
      }
      if (nodeId_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(NodeId);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (Kind != 0) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Kind);
      }
      if (nodeId_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(NodeId);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TreeWireNode other) {
      if (other == null) {
        return;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.Kind != 0) {
        Kind = other.Kind;
      }
      if (other.nodeId_ != null) {
        if (nodeId_ == null) {
          nodeId_ = new global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire();
        }
        NodeId.MergeFrom(other.NodeId);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            Name = input.ReadString();
            break;
          }
          case 16: {
            kind_ = (global::SourceCode.Chasm.IO.Proto.Wire.NodeKindWire) input.ReadEnum();
            break;
          }
          case 26: {
            if (nodeId_ == null) {
              nodeId_ = new global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire();
            }
            input.ReadMessage(nodeId_);
            break;
          }
        }
      }
    }

  }

  /// <summary>
  /// Tree
  /// </summary>
  public sealed partial class TreeWire : pb::IMessage<TreeWire> {
    private static readonly pb::MessageParser<TreeWire> _parser = new pb::MessageParser<TreeWire>(() => new TreeWire());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TreeWire> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::SourceCode.Chasm.IO.Proto.Wire.WireReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TreeWire() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TreeWire(TreeWire other) : this() {
      nodes_ = other.nodes_.Clone();
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TreeWire Clone() {
      return new TreeWire(this);
    }

    /// <summary>Field number for the "Nodes" field.</summary>
    public const int NodesFieldNumber = 1;
    private static readonly pb::FieldCodec<global::SourceCode.Chasm.IO.Proto.Wire.TreeWireNode> _repeated_nodes_codec
        = pb::FieldCodec.ForMessage(10, global::SourceCode.Chasm.IO.Proto.Wire.TreeWireNode.Parser);
    private readonly pbc::RepeatedField<global::SourceCode.Chasm.IO.Proto.Wire.TreeWireNode> nodes_ = new pbc::RepeatedField<global::SourceCode.Chasm.IO.Proto.Wire.TreeWireNode>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::SourceCode.Chasm.IO.Proto.Wire.TreeWireNode> Nodes {
      get { return nodes_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TreeWire);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TreeWire other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!nodes_.Equals(other.nodes_)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= nodes_.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      nodes_.WriteTo(output, _repeated_nodes_codec);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += nodes_.CalculateSize(_repeated_nodes_codec);
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TreeWire other) {
      if (other == null) {
        return;
      }
      nodes_.Add(other.nodes_);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            nodes_.AddEntriesFrom(input, _repeated_nodes_codec);
            break;
          }
        }
      }
    }

  }

  /// <summary>
  /// Audit
  /// </summary>
  public sealed partial class AuditWire : pb::IMessage<AuditWire> {
    private static readonly pb::MessageParser<AuditWire> _parser = new pb::MessageParser<AuditWire>(() => new AuditWire());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<AuditWire> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::SourceCode.Chasm.IO.Proto.Wire.WireReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public AuditWire() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public AuditWire(AuditWire other) : this() {
      name_ = other.name_;
      time_ = other.time_;
      offset_ = other.offset_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public AuditWire Clone() {
      return new AuditWire(this);
    }

    /// <summary>Field number for the "Name" field.</summary>
    public const int NameFieldNumber = 1;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Time" field.</summary>
    public const int TimeFieldNumber = 2;
    private long time_;
    /// <summary>
    /// Milliseconds
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public long Time {
      get { return time_; }
      set {
        time_ = value;
      }
    }

    /// <summary>Field number for the "Offset" field.</summary>
    public const int OffsetFieldNumber = 3;
    private int offset_;
    /// <summary>
    /// Minutes
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Offset {
      get { return offset_; }
      set {
        offset_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as AuditWire);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(AuditWire other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Name != other.Name) return false;
      if (Time != other.Time) return false;
      if (Offset != other.Offset) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (Time != 0L) hash ^= Time.GetHashCode();
      if (Offset != 0) hash ^= Offset.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Name.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Name);
      }
      if (Time != 0L) {
        output.WriteRawTag(16);
        output.WriteInt64(Time);
      }
      if (Offset != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Offset);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (Time != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(Time);
      }
      if (Offset != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Offset);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(AuditWire other) {
      if (other == null) {
        return;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.Time != 0L) {
        Time = other.Time;
      }
      if (other.Offset != 0) {
        Offset = other.Offset;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            Name = input.ReadString();
            break;
          }
          case 16: {
            Time = input.ReadInt64();
            break;
          }
          case 24: {
            Offset = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  /// <summary>
  /// Commit
  /// </summary>
  public sealed partial class CommitWire : pb::IMessage<CommitWire> {
    private static readonly pb::MessageParser<CommitWire> _parser = new pb::MessageParser<CommitWire>(() => new CommitWire());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<CommitWire> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::SourceCode.Chasm.IO.Proto.Wire.WireReflection.Descriptor.MessageTypes[4]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CommitWire() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CommitWire(CommitWire other) : this() {
      parents_ = other.parents_.Clone();
      TreeId = other.treeId_ != null ? other.TreeId.Clone() : null;
      Author = other.author_ != null ? other.Author.Clone() : null;
      Committer = other.committer_ != null ? other.Committer.Clone() : null;
      message_ = other.message_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CommitWire Clone() {
      return new CommitWire(this);
    }

    /// <summary>Field number for the "Parents" field.</summary>
    public const int ParentsFieldNumber = 1;
    private static readonly pb::FieldCodec<global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire> _repeated_parents_codec
        = pb::FieldCodec.ForMessage(10, global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire.Parser);
    private readonly pbc::RepeatedField<global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire> parents_ = new pbc::RepeatedField<global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire> Parents {
      get { return parents_; }
    }

    /// <summary>Field number for the "TreeId" field.</summary>
    public const int TreeIdFieldNumber = 2;
    private global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire treeId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire TreeId {
      get { return treeId_; }
      set {
        treeId_ = value;
      }
    }

    /// <summary>Field number for the "Author" field.</summary>
    public const int AuthorFieldNumber = 3;
    private global::SourceCode.Chasm.IO.Proto.Wire.AuditWire author_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::SourceCode.Chasm.IO.Proto.Wire.AuditWire Author {
      get { return author_; }
      set {
        author_ = value;
      }
    }

    /// <summary>Field number for the "Committer" field.</summary>
    public const int CommitterFieldNumber = 4;
    private global::SourceCode.Chasm.IO.Proto.Wire.AuditWire committer_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::SourceCode.Chasm.IO.Proto.Wire.AuditWire Committer {
      get { return committer_; }
      set {
        committer_ = value;
      }
    }

    /// <summary>Field number for the "Message" field.</summary>
    public const int MessageFieldNumber = 5;
    private string message_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Message {
      get { return message_; }
      set {
        message_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as CommitWire);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(CommitWire other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!parents_.Equals(other.parents_)) return false;
      if (!object.Equals(TreeId, other.TreeId)) return false;
      if (!object.Equals(Author, other.Author)) return false;
      if (!object.Equals(Committer, other.Committer)) return false;
      if (Message != other.Message) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= parents_.GetHashCode();
      if (treeId_ != null) hash ^= TreeId.GetHashCode();
      if (author_ != null) hash ^= Author.GetHashCode();
      if (committer_ != null) hash ^= Committer.GetHashCode();
      if (Message.Length != 0) hash ^= Message.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      parents_.WriteTo(output, _repeated_parents_codec);
      if (treeId_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(TreeId);
      }
      if (author_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(Author);
      }
      if (committer_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(Committer);
      }
      if (Message.Length != 0) {
        output.WriteRawTag(42);
        output.WriteString(Message);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += parents_.CalculateSize(_repeated_parents_codec);
      if (treeId_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(TreeId);
      }
      if (author_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Author);
      }
      if (committer_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Committer);
      }
      if (Message.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Message);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(CommitWire other) {
      if (other == null) {
        return;
      }
      parents_.Add(other.parents_);
      if (other.treeId_ != null) {
        if (treeId_ == null) {
          treeId_ = new global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire();
        }
        TreeId.MergeFrom(other.TreeId);
      }
      if (other.author_ != null) {
        if (author_ == null) {
          author_ = new global::SourceCode.Chasm.IO.Proto.Wire.AuditWire();
        }
        Author.MergeFrom(other.Author);
      }
      if (other.committer_ != null) {
        if (committer_ == null) {
          committer_ = new global::SourceCode.Chasm.IO.Proto.Wire.AuditWire();
        }
        Committer.MergeFrom(other.Committer);
      }
      if (other.Message.Length != 0) {
        Message = other.Message;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            parents_.AddEntriesFrom(input, _repeated_parents_codec);
            break;
          }
          case 18: {
            if (treeId_ == null) {
              treeId_ = new global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire();
            }
            input.ReadMessage(treeId_);
            break;
          }
          case 26: {
            if (author_ == null) {
              author_ = new global::SourceCode.Chasm.IO.Proto.Wire.AuditWire();
            }
            input.ReadMessage(author_);
            break;
          }
          case 34: {
            if (committer_ == null) {
              committer_ = new global::SourceCode.Chasm.IO.Proto.Wire.AuditWire();
            }
            input.ReadMessage(committer_);
            break;
          }
          case 42: {
            Message = input.ReadString();
            break;
          }
        }
      }
    }

  }

  /// <summary>
  /// CommitId
  /// </summary>
  public sealed partial class CommitIdWire : pb::IMessage<CommitIdWire> {
    private static readonly pb::MessageParser<CommitIdWire> _parser = new pb::MessageParser<CommitIdWire>(() => new CommitIdWire());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<CommitIdWire> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::SourceCode.Chasm.IO.Proto.Wire.WireReflection.Descriptor.MessageTypes[5]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CommitIdWire() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CommitIdWire(CommitIdWire other) : this() {
      Id = other.id_ != null ? other.Id.Clone() : null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CommitIdWire Clone() {
      return new CommitIdWire(this);
    }

    /// <summary>Field number for the "Id" field.</summary>
    public const int IdFieldNumber = 1;
    private global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire id_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as CommitIdWire);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(CommitIdWire other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Id, other.Id)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (id_ != null) hash ^= Id.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (id_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Id);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (id_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Id);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(CommitIdWire other) {
      if (other == null) {
        return;
      }
      if (other.id_ != null) {
        if (id_ == null) {
          id_ = new global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire();
        }
        Id.MergeFrom(other.Id);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            if (id_ == null) {
              id_ = new global::SourceCode.Chasm.IO.Proto.Wire.Sha1Wire();
            }
            input.ReadMessage(id_);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
