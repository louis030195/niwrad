// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: github.com/louis030195/protometry/api/volume/volume.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Protometry.Volume {

  /// <summary>Holder for reflection information generated from github.com/louis030195/protometry/api/volume/volume.proto</summary>
  public static partial class VolumeReflection {

    #region Descriptor
    /// <summary>File descriptor for github.com/louis030195/protometry/api/volume/volume.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static VolumeReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CjlnaXRodWIuY29tL2xvdWlzMDMwMTk1L3Byb3RvbWV0cnkvYXBpL3ZvbHVt",
            "ZS92b2x1bWUucHJvdG8SEXByb3RvbWV0cnkudm9sdW1lGjtnaXRodWIuY29t",
            "L2xvdWlzMDMwMTk1L3Byb3RvbWV0cnkvYXBpL3ZlY3RvcjMvdmVjdG9yMy5w",
            "cm90byJFCgZTcGhlcmUSKwoGY2VudGVyGAEgASgLMhsucHJvdG9tZXRyeS52",
            "ZWN0b3IzLlZlY3RvcjMSDgoGcmFkaXVzGAIgASgBIkUKB0NhcHN1bGUSKwoG",
            "Y2VudGVyGAEgASgLMhsucHJvdG9tZXRyeS52ZWN0b3IzLlZlY3RvcjMSDQoF",
            "d2lkdGgYAiABKAEiWQoDQm94EigKA21pbhgBIAEoCzIbLnByb3RvbWV0cnku",
            "dmVjdG9yMy5WZWN0b3IzEigKA21heBgCIAEoCzIbLnByb3RvbWV0cnkudmVj",
            "dG9yMy5WZWN0b3IzIsgBCgRNZXNoEisKBmNlbnRlchgBIAEoCzIbLnByb3Rv",
            "bWV0cnkudmVjdG9yMy5WZWN0b3IzEi0KCHZlcnRpY2VzGAIgAygLMhsucHJv",
            "dG9tZXRyeS52ZWN0b3IzLlZlY3RvcjMSDAoEdHJpcxgDIAMoBRIsCgdub3Jt",
            "YWxzGAQgAygLMhsucHJvdG9tZXRyeS52ZWN0b3IzLlZlY3RvcjMSKAoDdXZz",
            "GAUgAygLMhsucHJvdG9tZXRyeS52ZWN0b3IzLlZlY3RvcjNCPQoVY29tLnBy",
            "b3RvbWV0cnkudm9sdW1lQgZWb2x1bWVQAVoGdm9sdW1lqgIRUHJvdG9tZXRy",
            "eS5Wb2x1bWViBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Protometry.Vector3.Vector3Reflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Protometry.Volume.Sphere), global::Protometry.Volume.Sphere.Parser, new[]{ "Center", "Radius" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Protometry.Volume.Capsule), global::Protometry.Volume.Capsule.Parser, new[]{ "Center", "Width" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Protometry.Volume.Box), global::Protometry.Volume.Box.Parser, new[]{ "Min", "Max" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Protometry.Volume.Mesh), global::Protometry.Volume.Mesh.Parser, new[]{ "Center", "Vertices", "Tris", "Normals", "Uvs" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Sphere : pb::IMessage<Sphere> {
    private static readonly pb::MessageParser<Sphere> _parser = new pb::MessageParser<Sphere>(() => new Sphere());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Sphere> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Protometry.Volume.VolumeReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Sphere() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Sphere(Sphere other) : this() {
      center_ = other.center_ != null ? other.center_.Clone() : null;
      radius_ = other.radius_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Sphere Clone() {
      return new Sphere(this);
    }

    /// <summary>Field number for the "center" field.</summary>
    public const int CenterFieldNumber = 1;
    private global::Protometry.Vector3.Vector3 center_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Protometry.Vector3.Vector3 Center {
      get { return center_; }
      set {
        center_ = value;
      }
    }

    /// <summary>Field number for the "radius" field.</summary>
    public const int RadiusFieldNumber = 2;
    private double radius_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public double Radius {
      get { return radius_; }
      set {
        radius_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Sphere);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Sphere other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Center, other.Center)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Radius, other.Radius)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (center_ != null) hash ^= Center.GetHashCode();
      if (Radius != 0D) hash ^= pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Radius);
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (center_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Center);
      }
      if (Radius != 0D) {
        output.WriteRawTag(17);
        output.WriteDouble(Radius);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (center_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Center);
      }
      if (Radius != 0D) {
        size += 1 + 8;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Sphere other) {
      if (other == null) {
        return;
      }
      if (other.center_ != null) {
        if (center_ == null) {
          Center = new global::Protometry.Vector3.Vector3();
        }
        Center.MergeFrom(other.Center);
      }
      if (other.Radius != 0D) {
        Radius = other.Radius;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (center_ == null) {
              Center = new global::Protometry.Vector3.Vector3();
            }
            input.ReadMessage(Center);
            break;
          }
          case 17: {
            Radius = input.ReadDouble();
            break;
          }
        }
      }
    }

  }

  public sealed partial class Capsule : pb::IMessage<Capsule> {
    private static readonly pb::MessageParser<Capsule> _parser = new pb::MessageParser<Capsule>(() => new Capsule());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Capsule> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Protometry.Volume.VolumeReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Capsule() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Capsule(Capsule other) : this() {
      center_ = other.center_ != null ? other.center_.Clone() : null;
      width_ = other.width_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Capsule Clone() {
      return new Capsule(this);
    }

    /// <summary>Field number for the "center" field.</summary>
    public const int CenterFieldNumber = 1;
    private global::Protometry.Vector3.Vector3 center_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Protometry.Vector3.Vector3 Center {
      get { return center_; }
      set {
        center_ = value;
      }
    }

    /// <summary>Field number for the "width" field.</summary>
    public const int WidthFieldNumber = 2;
    private double width_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public double Width {
      get { return width_; }
      set {
        width_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Capsule);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Capsule other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Center, other.Center)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Width, other.Width)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (center_ != null) hash ^= Center.GetHashCode();
      if (Width != 0D) hash ^= pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Width);
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (center_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Center);
      }
      if (Width != 0D) {
        output.WriteRawTag(17);
        output.WriteDouble(Width);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (center_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Center);
      }
      if (Width != 0D) {
        size += 1 + 8;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Capsule other) {
      if (other == null) {
        return;
      }
      if (other.center_ != null) {
        if (center_ == null) {
          Center = new global::Protometry.Vector3.Vector3();
        }
        Center.MergeFrom(other.Center);
      }
      if (other.Width != 0D) {
        Width = other.Width;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (center_ == null) {
              Center = new global::Protometry.Vector3.Vector3();
            }
            input.ReadMessage(Center);
            break;
          }
          case 17: {
            Width = input.ReadDouble();
            break;
          }
        }
      }
    }

  }

  /// <summary>
  /// Box is an AABB volume
  /// </summary>
  public sealed partial class Box : pb::IMessage<Box> {
    private static readonly pb::MessageParser<Box> _parser = new pb::MessageParser<Box>(() => new Box());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Box> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Protometry.Volume.VolumeReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Box() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Box(Box other) : this() {
      min_ = other.min_ != null ? other.min_.Clone() : null;
      max_ = other.max_ != null ? other.max_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Box Clone() {
      return new Box(this);
    }

    /// <summary>Field number for the "min" field.</summary>
    public const int MinFieldNumber = 1;
    private global::Protometry.Vector3.Vector3 min_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Protometry.Vector3.Vector3 Min {
      get { return min_; }
      set {
        min_ = value;
      }
    }

    /// <summary>Field number for the "max" field.</summary>
    public const int MaxFieldNumber = 2;
    private global::Protometry.Vector3.Vector3 max_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Protometry.Vector3.Vector3 Max {
      get { return max_; }
      set {
        max_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Box);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Box other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Min, other.Min)) return false;
      if (!object.Equals(Max, other.Max)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (min_ != null) hash ^= Min.GetHashCode();
      if (max_ != null) hash ^= Max.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (min_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Min);
      }
      if (max_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Max);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (min_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Min);
      }
      if (max_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Max);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Box other) {
      if (other == null) {
        return;
      }
      if (other.min_ != null) {
        if (min_ == null) {
          Min = new global::Protometry.Vector3.Vector3();
        }
        Min.MergeFrom(other.Min);
      }
      if (other.max_ != null) {
        if (max_ == null) {
          Max = new global::Protometry.Vector3.Vector3();
        }
        Max.MergeFrom(other.Max);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (min_ == null) {
              Min = new global::Protometry.Vector3.Vector3();
            }
            input.ReadMessage(Min);
            break;
          }
          case 18: {
            if (max_ == null) {
              Max = new global::Protometry.Vector3.Vector3();
            }
            input.ReadMessage(Max);
            break;
          }
        }
      }
    }

  }

  public sealed partial class Mesh : pb::IMessage<Mesh> {
    private static readonly pb::MessageParser<Mesh> _parser = new pb::MessageParser<Mesh>(() => new Mesh());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Mesh> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Protometry.Volume.VolumeReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Mesh() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Mesh(Mesh other) : this() {
      center_ = other.center_ != null ? other.center_.Clone() : null;
      vertices_ = other.vertices_.Clone();
      tris_ = other.tris_.Clone();
      normals_ = other.normals_.Clone();
      uvs_ = other.uvs_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Mesh Clone() {
      return new Mesh(this);
    }

    /// <summary>Field number for the "center" field.</summary>
    public const int CenterFieldNumber = 1;
    private global::Protometry.Vector3.Vector3 center_;
    /// <summary>
    /// I.e "pivot"
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Protometry.Vector3.Vector3 Center {
      get { return center_; }
      set {
        center_ = value;
      }
    }

    /// <summary>Field number for the "vertices" field.</summary>
    public const int VerticesFieldNumber = 2;
    private static readonly pb::FieldCodec<global::Protometry.Vector3.Vector3> _repeated_vertices_codec
        = pb::FieldCodec.ForMessage(18, global::Protometry.Vector3.Vector3.Parser);
    private readonly pbc::RepeatedField<global::Protometry.Vector3.Vector3> vertices_ = new pbc::RepeatedField<global::Protometry.Vector3.Vector3>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Protometry.Vector3.Vector3> Vertices {
      get { return vertices_; }
    }

    /// <summary>Field number for the "tris" field.</summary>
    public const int TrisFieldNumber = 3;
    private static readonly pb::FieldCodec<int> _repeated_tris_codec
        = pb::FieldCodec.ForInt32(26);
    private readonly pbc::RepeatedField<int> tris_ = new pbc::RepeatedField<int>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<int> Tris {
      get { return tris_; }
    }

    /// <summary>Field number for the "normals" field.</summary>
    public const int NormalsFieldNumber = 4;
    private static readonly pb::FieldCodec<global::Protometry.Vector3.Vector3> _repeated_normals_codec
        = pb::FieldCodec.ForMessage(34, global::Protometry.Vector3.Vector3.Parser);
    private readonly pbc::RepeatedField<global::Protometry.Vector3.Vector3> normals_ = new pbc::RepeatedField<global::Protometry.Vector3.Vector3>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Protometry.Vector3.Vector3> Normals {
      get { return normals_; }
    }

    /// <summary>Field number for the "uvs" field.</summary>
    public const int UvsFieldNumber = 5;
    private static readonly pb::FieldCodec<global::Protometry.Vector3.Vector3> _repeated_uvs_codec
        = pb::FieldCodec.ForMessage(42, global::Protometry.Vector3.Vector3.Parser);
    private readonly pbc::RepeatedField<global::Protometry.Vector3.Vector3> uvs_ = new pbc::RepeatedField<global::Protometry.Vector3.Vector3>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Protometry.Vector3.Vector3> Uvs {
      get { return uvs_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Mesh);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Mesh other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Center, other.Center)) return false;
      if(!vertices_.Equals(other.vertices_)) return false;
      if(!tris_.Equals(other.tris_)) return false;
      if(!normals_.Equals(other.normals_)) return false;
      if(!uvs_.Equals(other.uvs_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (center_ != null) hash ^= Center.GetHashCode();
      hash ^= vertices_.GetHashCode();
      hash ^= tris_.GetHashCode();
      hash ^= normals_.GetHashCode();
      hash ^= uvs_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (center_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Center);
      }
      vertices_.WriteTo(output, _repeated_vertices_codec);
      tris_.WriteTo(output, _repeated_tris_codec);
      normals_.WriteTo(output, _repeated_normals_codec);
      uvs_.WriteTo(output, _repeated_uvs_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (center_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Center);
      }
      size += vertices_.CalculateSize(_repeated_vertices_codec);
      size += tris_.CalculateSize(_repeated_tris_codec);
      size += normals_.CalculateSize(_repeated_normals_codec);
      size += uvs_.CalculateSize(_repeated_uvs_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Mesh other) {
      if (other == null) {
        return;
      }
      if (other.center_ != null) {
        if (center_ == null) {
          Center = new global::Protometry.Vector3.Vector3();
        }
        Center.MergeFrom(other.Center);
      }
      vertices_.Add(other.vertices_);
      tris_.Add(other.tris_);
      normals_.Add(other.normals_);
      uvs_.Add(other.uvs_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (center_ == null) {
              Center = new global::Protometry.Vector3.Vector3();
            }
            input.ReadMessage(Center);
            break;
          }
          case 18: {
            vertices_.AddEntriesFrom(input, _repeated_vertices_codec);
            break;
          }
          case 26:
          case 24: {
            tris_.AddEntriesFrom(input, _repeated_tris_codec);
            break;
          }
          case 34: {
            normals_.AddEntriesFrom(input, _repeated_normals_codec);
            break;
          }
          case 42: {
            uvs_.AddEntriesFrom(input, _repeated_uvs_codec);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
