// Code generated by protoc-gen-go. DO NOT EDIT.
// source: realtime.proto

package realtime

import (
	fmt "fmt"
	proto "github.com/golang/protobuf/proto"
	quaternion "github.com/louis030195/protometry/api/quaternion"
	vector3 "github.com/louis030195/protometry/api/vector3"
	volume "github.com/louis030195/protometry/api/volume"
	math "math"
)

// Reference imports to suppress errors if they are not otherwise used.
var _ = proto.Marshal
var _ = fmt.Errorf
var _ = math.Inf

// This is a compile-time assertion to ensure that this generated file
// is compatible with the proto package it is being compiled against.
// A compilation error at this line likely means your copy of the
// proto package needs to be updated.
const _ = proto.ProtoPackageIsVersion3 // please upgrade the proto package

// TODO: kind of ugly to use same struct for client -> server and server -> client ?
type Packet struct {
	SenderId   string   `protobuf:"bytes,1,opt,name=sender_id,json=senderId,proto3" json:"sender_id,omitempty"`
	IsServer   bool     `protobuf:"varint,2,opt,name=is_server,json=isServer,proto3" json:"is_server,omitempty"`
	Recipients []string `protobuf:"bytes,3,rep,name=recipients,proto3" json:"recipients,omitempty"`
	// TODO: prob will have to switch to a box instead of vector3 later (want to notify before arrival ...)
	Impact *vector3.Vector3 `protobuf:"bytes,4,opt,name=impact,proto3" json:"impact,omitempty"`
	// Types that are valid to be assigned to Type:
	//	*Packet_MatchJoin
	//	*Packet_Map
	//	*Packet_UpdateTransform
	//	*Packet_NavMeshUpdate
	//	*Packet_Spawn
	//	*Packet_RequestSpawn
	//	*Packet_Destroy
	//	*Packet_RequestDestroy
	//	*Packet_Meme
	//	*Packet_Initialized
	Type                 isPacket_Type `protobuf_oneof:"type"`
	XXX_NoUnkeyedLiteral struct{}      `json:"-"`
	XXX_unrecognized     []byte        `json:"-"`
	XXX_sizecache        int32         `json:"-"`
}

func (m *Packet) Reset()         { *m = Packet{} }
func (m *Packet) String() string { return proto.CompactTextString(m) }
func (*Packet) ProtoMessage()    {}
func (*Packet) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{0}
}

func (m *Packet) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_Packet.Unmarshal(m, b)
}
func (m *Packet) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_Packet.Marshal(b, m, deterministic)
}
func (m *Packet) XXX_Merge(src proto.Message) {
	xxx_messageInfo_Packet.Merge(m, src)
}
func (m *Packet) XXX_Size() int {
	return xxx_messageInfo_Packet.Size(m)
}
func (m *Packet) XXX_DiscardUnknown() {
	xxx_messageInfo_Packet.DiscardUnknown(m)
}

var xxx_messageInfo_Packet proto.InternalMessageInfo

func (m *Packet) GetSenderId() string {
	if m != nil {
		return m.SenderId
	}
	return ""
}

func (m *Packet) GetIsServer() bool {
	if m != nil {
		return m.IsServer
	}
	return false
}

func (m *Packet) GetRecipients() []string {
	if m != nil {
		return m.Recipients
	}
	return nil
}

func (m *Packet) GetImpact() *vector3.Vector3 {
	if m != nil {
		return m.Impact
	}
	return nil
}

type isPacket_Type interface {
	isPacket_Type()
}

type Packet_MatchJoin struct {
	MatchJoin *MatchJoin `protobuf:"bytes,7,opt,name=match_join,json=matchJoin,proto3,oneof"`
}

type Packet_Map struct {
	Map *Map `protobuf:"bytes,8,opt,name=map,proto3,oneof"`
}

type Packet_UpdateTransform struct {
	UpdateTransform *UpdateTransform `protobuf:"bytes,10,opt,name=update_transform,json=updateTransform,proto3,oneof"`
}

type Packet_NavMeshUpdate struct {
	NavMeshUpdate *NavMeshUpdate `protobuf:"bytes,11,opt,name=nav_mesh_update,json=navMeshUpdate,proto3,oneof"`
}

type Packet_Spawn struct {
	Spawn *Spawn `protobuf:"bytes,15,opt,name=spawn,proto3,oneof"`
}

type Packet_RequestSpawn struct {
	RequestSpawn *Spawn `protobuf:"bytes,16,opt,name=request_spawn,json=requestSpawn,proto3,oneof"`
}

type Packet_Destroy struct {
	Destroy *Destroy `protobuf:"bytes,17,opt,name=destroy,proto3,oneof"`
}

type Packet_RequestDestroy struct {
	RequestDestroy *Destroy `protobuf:"bytes,18,opt,name=request_destroy,json=requestDestroy,proto3,oneof"`
}

type Packet_Meme struct {
	Meme *Meme `protobuf:"bytes,19,opt,name=meme,proto3,oneof"`
}

type Packet_Initialized struct {
	Initialized *Initialized `protobuf:"bytes,25,opt,name=initialized,proto3,oneof"`
}

func (*Packet_MatchJoin) isPacket_Type() {}

func (*Packet_Map) isPacket_Type() {}

func (*Packet_UpdateTransform) isPacket_Type() {}

func (*Packet_NavMeshUpdate) isPacket_Type() {}

func (*Packet_Spawn) isPacket_Type() {}

func (*Packet_RequestSpawn) isPacket_Type() {}

func (*Packet_Destroy) isPacket_Type() {}

func (*Packet_RequestDestroy) isPacket_Type() {}

func (*Packet_Meme) isPacket_Type() {}

func (*Packet_Initialized) isPacket_Type() {}

func (m *Packet) GetType() isPacket_Type {
	if m != nil {
		return m.Type
	}
	return nil
}

func (m *Packet) GetMatchJoin() *MatchJoin {
	if x, ok := m.GetType().(*Packet_MatchJoin); ok {
		return x.MatchJoin
	}
	return nil
}

func (m *Packet) GetMap() *Map {
	if x, ok := m.GetType().(*Packet_Map); ok {
		return x.Map
	}
	return nil
}

func (m *Packet) GetUpdateTransform() *UpdateTransform {
	if x, ok := m.GetType().(*Packet_UpdateTransform); ok {
		return x.UpdateTransform
	}
	return nil
}

func (m *Packet) GetNavMeshUpdate() *NavMeshUpdate {
	if x, ok := m.GetType().(*Packet_NavMeshUpdate); ok {
		return x.NavMeshUpdate
	}
	return nil
}

func (m *Packet) GetSpawn() *Spawn {
	if x, ok := m.GetType().(*Packet_Spawn); ok {
		return x.Spawn
	}
	return nil
}

func (m *Packet) GetRequestSpawn() *Spawn {
	if x, ok := m.GetType().(*Packet_RequestSpawn); ok {
		return x.RequestSpawn
	}
	return nil
}

func (m *Packet) GetDestroy() *Destroy {
	if x, ok := m.GetType().(*Packet_Destroy); ok {
		return x.Destroy
	}
	return nil
}

func (m *Packet) GetRequestDestroy() *Destroy {
	if x, ok := m.GetType().(*Packet_RequestDestroy); ok {
		return x.RequestDestroy
	}
	return nil
}

func (m *Packet) GetMeme() *Meme {
	if x, ok := m.GetType().(*Packet_Meme); ok {
		return x.Meme
	}
	return nil
}

func (m *Packet) GetInitialized() *Initialized {
	if x, ok := m.GetType().(*Packet_Initialized); ok {
		return x.Initialized
	}
	return nil
}

// XXX_OneofWrappers is for the internal use of the proto package.
func (*Packet) XXX_OneofWrappers() []interface{} {
	return []interface{}{
		(*Packet_MatchJoin)(nil),
		(*Packet_Map)(nil),
		(*Packet_UpdateTransform)(nil),
		(*Packet_NavMeshUpdate)(nil),
		(*Packet_Spawn)(nil),
		(*Packet_RequestSpawn)(nil),
		(*Packet_Destroy)(nil),
		(*Packet_RequestDestroy)(nil),
		(*Packet_Meme)(nil),
		(*Packet_Initialized)(nil),
	}
}

type MatchJoin struct {
	Region               *volume.Box `protobuf:"bytes,1,opt,name=region,proto3" json:"region,omitempty"`
	Seed                 int64       `protobuf:"varint,2,opt,name=seed,proto3" json:"seed,omitempty"`
	XXX_NoUnkeyedLiteral struct{}    `json:"-"`
	XXX_unrecognized     []byte      `json:"-"`
	XXX_sizecache        int32       `json:"-"`
}

func (m *MatchJoin) Reset()         { *m = MatchJoin{} }
func (m *MatchJoin) String() string { return proto.CompactTextString(m) }
func (*MatchJoin) ProtoMessage()    {}
func (*MatchJoin) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{1}
}

func (m *MatchJoin) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_MatchJoin.Unmarshal(m, b)
}
func (m *MatchJoin) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_MatchJoin.Marshal(b, m, deterministic)
}
func (m *MatchJoin) XXX_Merge(src proto.Message) {
	xxx_messageInfo_MatchJoin.Merge(m, src)
}
func (m *MatchJoin) XXX_Size() int {
	return xxx_messageInfo_MatchJoin.Size(m)
}
func (m *MatchJoin) XXX_DiscardUnknown() {
	xxx_messageInfo_MatchJoin.DiscardUnknown(m)
}

var xxx_messageInfo_MatchJoin proto.InternalMessageInfo

func (m *MatchJoin) GetRegion() *volume.Box {
	if m != nil {
		return m.Region
	}
	return nil
}

func (m *MatchJoin) GetSeed() int64 {
	if m != nil {
		return m.Seed
	}
	return 0
}

type Map struct {
	// Offsets of the chunk of map data
	XBase                int64    `protobuf:"varint,1,opt,name=x_base,json=xBase,proto3" json:"x_base,omitempty"`
	YBase                int64    `protobuf:"varint,2,opt,name=y_base,json=yBase,proto3" json:"y_base,omitempty"`
	Map                  *Matrix  `protobuf:"bytes,3,opt,name=map,proto3" json:"map,omitempty"`
	XXX_NoUnkeyedLiteral struct{} `json:"-"`
	XXX_unrecognized     []byte   `json:"-"`
	XXX_sizecache        int32    `json:"-"`
}

func (m *Map) Reset()         { *m = Map{} }
func (m *Map) String() string { return proto.CompactTextString(m) }
func (*Map) ProtoMessage()    {}
func (*Map) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{2}
}

func (m *Map) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_Map.Unmarshal(m, b)
}
func (m *Map) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_Map.Marshal(b, m, deterministic)
}
func (m *Map) XXX_Merge(src proto.Message) {
	xxx_messageInfo_Map.Merge(m, src)
}
func (m *Map) XXX_Size() int {
	return xxx_messageInfo_Map.Size(m)
}
func (m *Map) XXX_DiscardUnknown() {
	xxx_messageInfo_Map.DiscardUnknown(m)
}

var xxx_messageInfo_Map proto.InternalMessageInfo

func (m *Map) GetXBase() int64 {
	if m != nil {
		return m.XBase
	}
	return 0
}

func (m *Map) GetYBase() int64 {
	if m != nil {
		return m.YBase
	}
	return 0
}

func (m *Map) GetMap() *Matrix {
	if m != nil {
		return m.Map
	}
	return nil
}

type Matrix struct {
	Rows                 []*Array `protobuf:"bytes,1,rep,name=rows,proto3" json:"rows,omitempty"`
	XXX_NoUnkeyedLiteral struct{} `json:"-"`
	XXX_unrecognized     []byte   `json:"-"`
	XXX_sizecache        int32    `json:"-"`
}

func (m *Matrix) Reset()         { *m = Matrix{} }
func (m *Matrix) String() string { return proto.CompactTextString(m) }
func (*Matrix) ProtoMessage()    {}
func (*Matrix) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{3}
}

func (m *Matrix) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_Matrix.Unmarshal(m, b)
}
func (m *Matrix) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_Matrix.Marshal(b, m, deterministic)
}
func (m *Matrix) XXX_Merge(src proto.Message) {
	xxx_messageInfo_Matrix.Merge(m, src)
}
func (m *Matrix) XXX_Size() int {
	return xxx_messageInfo_Matrix.Size(m)
}
func (m *Matrix) XXX_DiscardUnknown() {
	xxx_messageInfo_Matrix.DiscardUnknown(m)
}

var xxx_messageInfo_Matrix proto.InternalMessageInfo

func (m *Matrix) GetRows() []*Array {
	if m != nil {
		return m.Rows
	}
	return nil
}

type Array struct {
	Cols                 []float64 `protobuf:"fixed64,1,rep,packed,name=cols,proto3" json:"cols,omitempty"`
	XXX_NoUnkeyedLiteral struct{}  `json:"-"`
	XXX_unrecognized     []byte    `json:"-"`
	XXX_sizecache        int32     `json:"-"`
}

func (m *Array) Reset()         { *m = Array{} }
func (m *Array) String() string { return proto.CompactTextString(m) }
func (*Array) ProtoMessage()    {}
func (*Array) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{4}
}

func (m *Array) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_Array.Unmarshal(m, b)
}
func (m *Array) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_Array.Marshal(b, m, deterministic)
}
func (m *Array) XXX_Merge(src proto.Message) {
	xxx_messageInfo_Array.Merge(m, src)
}
func (m *Array) XXX_Size() int {
	return xxx_messageInfo_Array.Size(m)
}
func (m *Array) XXX_DiscardUnknown() {
	xxx_messageInfo_Array.DiscardUnknown(m)
}

var xxx_messageInfo_Array proto.InternalMessageInfo

func (m *Array) GetCols() []float64 {
	if m != nil {
		return m.Cols
	}
	return nil
}

// General purpose transform update typically shared by several different types of objects
type UpdateTransform struct {
	Transform            *Transform `protobuf:"bytes,1,opt,name=transform,proto3" json:"transform,omitempty"`
	XXX_NoUnkeyedLiteral struct{}   `json:"-"`
	XXX_unrecognized     []byte     `json:"-"`
	XXX_sizecache        int32      `json:"-"`
}

func (m *UpdateTransform) Reset()         { *m = UpdateTransform{} }
func (m *UpdateTransform) String() string { return proto.CompactTextString(m) }
func (*UpdateTransform) ProtoMessage()    {}
func (*UpdateTransform) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{5}
}

func (m *UpdateTransform) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_UpdateTransform.Unmarshal(m, b)
}
func (m *UpdateTransform) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_UpdateTransform.Marshal(b, m, deterministic)
}
func (m *UpdateTransform) XXX_Merge(src proto.Message) {
	xxx_messageInfo_UpdateTransform.Merge(m, src)
}
func (m *UpdateTransform) XXX_Size() int {
	return xxx_messageInfo_UpdateTransform.Size(m)
}
func (m *UpdateTransform) XXX_DiscardUnknown() {
	xxx_messageInfo_UpdateTransform.DiscardUnknown(m)
}

var xxx_messageInfo_UpdateTransform proto.InternalMessageInfo

func (m *UpdateTransform) GetTransform() *Transform {
	if m != nil {
		return m.Transform
	}
	return nil
}

type NavMeshUpdate struct {
	Id                   uint64           `protobuf:"varint,1,opt,name=id,proto3" json:"id,omitempty"`
	Destination          *vector3.Vector3 `protobuf:"bytes,2,opt,name=destination,proto3" json:"destination,omitempty"`
	XXX_NoUnkeyedLiteral struct{}         `json:"-"`
	XXX_unrecognized     []byte           `json:"-"`
	XXX_sizecache        int32            `json:"-"`
}

func (m *NavMeshUpdate) Reset()         { *m = NavMeshUpdate{} }
func (m *NavMeshUpdate) String() string { return proto.CompactTextString(m) }
func (*NavMeshUpdate) ProtoMessage()    {}
func (*NavMeshUpdate) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{6}
}

func (m *NavMeshUpdate) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_NavMeshUpdate.Unmarshal(m, b)
}
func (m *NavMeshUpdate) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_NavMeshUpdate.Marshal(b, m, deterministic)
}
func (m *NavMeshUpdate) XXX_Merge(src proto.Message) {
	xxx_messageInfo_NavMeshUpdate.Merge(m, src)
}
func (m *NavMeshUpdate) XXX_Size() int {
	return xxx_messageInfo_NavMeshUpdate.Size(m)
}
func (m *NavMeshUpdate) XXX_DiscardUnknown() {
	xxx_messageInfo_NavMeshUpdate.DiscardUnknown(m)
}

var xxx_messageInfo_NavMeshUpdate proto.InternalMessageInfo

func (m *NavMeshUpdate) GetId() uint64 {
	if m != nil {
		return m.Id
	}
	return 0
}

func (m *NavMeshUpdate) GetDestination() *vector3.Vector3 {
	if m != nil {
		return m.Destination
	}
	return nil
}

type Spawn struct {
	// Types that are valid to be assigned to Type:
	//	*Spawn_Any
	//	*Spawn_Tree
	//	*Spawn_Animal
	Type                 isSpawn_Type `protobuf_oneof:"type"`
	XXX_NoUnkeyedLiteral struct{}     `json:"-"`
	XXX_unrecognized     []byte       `json:"-"`
	XXX_sizecache        int32        `json:"-"`
}

func (m *Spawn) Reset()         { *m = Spawn{} }
func (m *Spawn) String() string { return proto.CompactTextString(m) }
func (*Spawn) ProtoMessage()    {}
func (*Spawn) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{7}
}

func (m *Spawn) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_Spawn.Unmarshal(m, b)
}
func (m *Spawn) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_Spawn.Marshal(b, m, deterministic)
}
func (m *Spawn) XXX_Merge(src proto.Message) {
	xxx_messageInfo_Spawn.Merge(m, src)
}
func (m *Spawn) XXX_Size() int {
	return xxx_messageInfo_Spawn.Size(m)
}
func (m *Spawn) XXX_DiscardUnknown() {
	xxx_messageInfo_Spawn.DiscardUnknown(m)
}

var xxx_messageInfo_Spawn proto.InternalMessageInfo

type isSpawn_Type interface {
	isSpawn_Type()
}

type Spawn_Any struct {
	Any *Transform `protobuf:"bytes,1,opt,name=any,proto3,oneof"`
}

type Spawn_Tree struct {
	Tree *Tree `protobuf:"bytes,2,opt,name=tree,proto3,oneof"`
}

type Spawn_Animal struct {
	Animal *Animal `protobuf:"bytes,3,opt,name=animal,proto3,oneof"`
}

func (*Spawn_Any) isSpawn_Type() {}

func (*Spawn_Tree) isSpawn_Type() {}

func (*Spawn_Animal) isSpawn_Type() {}

func (m *Spawn) GetType() isSpawn_Type {
	if m != nil {
		return m.Type
	}
	return nil
}

func (m *Spawn) GetAny() *Transform {
	if x, ok := m.GetType().(*Spawn_Any); ok {
		return x.Any
	}
	return nil
}

func (m *Spawn) GetTree() *Tree {
	if x, ok := m.GetType().(*Spawn_Tree); ok {
		return x.Tree
	}
	return nil
}

func (m *Spawn) GetAnimal() *Animal {
	if x, ok := m.GetType().(*Spawn_Animal); ok {
		return x.Animal
	}
	return nil
}

// XXX_OneofWrappers is for the internal use of the proto package.
func (*Spawn) XXX_OneofWrappers() []interface{} {
	return []interface{}{
		(*Spawn_Any)(nil),
		(*Spawn_Tree)(nil),
		(*Spawn_Animal)(nil),
	}
}

type Destroy struct {
	// Types that are valid to be assigned to Type:
	//	*Destroy_Any
	//	*Destroy_Tree
	//	*Destroy_Animal
	Type                 isDestroy_Type `protobuf_oneof:"type"`
	XXX_NoUnkeyedLiteral struct{}       `json:"-"`
	XXX_unrecognized     []byte         `json:"-"`
	XXX_sizecache        int32          `json:"-"`
}

func (m *Destroy) Reset()         { *m = Destroy{} }
func (m *Destroy) String() string { return proto.CompactTextString(m) }
func (*Destroy) ProtoMessage()    {}
func (*Destroy) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{8}
}

func (m *Destroy) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_Destroy.Unmarshal(m, b)
}
func (m *Destroy) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_Destroy.Marshal(b, m, deterministic)
}
func (m *Destroy) XXX_Merge(src proto.Message) {
	xxx_messageInfo_Destroy.Merge(m, src)
}
func (m *Destroy) XXX_Size() int {
	return xxx_messageInfo_Destroy.Size(m)
}
func (m *Destroy) XXX_DiscardUnknown() {
	xxx_messageInfo_Destroy.DiscardUnknown(m)
}

var xxx_messageInfo_Destroy proto.InternalMessageInfo

type isDestroy_Type interface {
	isDestroy_Type()
}

type Destroy_Any struct {
	Any *Transform `protobuf:"bytes,1,opt,name=any,proto3,oneof"`
}

type Destroy_Tree struct {
	Tree *Tree `protobuf:"bytes,2,opt,name=tree,proto3,oneof"`
}

type Destroy_Animal struct {
	Animal *Animal `protobuf:"bytes,3,opt,name=animal,proto3,oneof"`
}

func (*Destroy_Any) isDestroy_Type() {}

func (*Destroy_Tree) isDestroy_Type() {}

func (*Destroy_Animal) isDestroy_Type() {}

func (m *Destroy) GetType() isDestroy_Type {
	if m != nil {
		return m.Type
	}
	return nil
}

func (m *Destroy) GetAny() *Transform {
	if x, ok := m.GetType().(*Destroy_Any); ok {
		return x.Any
	}
	return nil
}

func (m *Destroy) GetTree() *Tree {
	if x, ok := m.GetType().(*Destroy_Tree); ok {
		return x.Tree
	}
	return nil
}

func (m *Destroy) GetAnimal() *Animal {
	if x, ok := m.GetType().(*Destroy_Animal); ok {
		return x.Animal
	}
	return nil
}

// XXX_OneofWrappers is for the internal use of the proto package.
func (*Destroy) XXX_OneofWrappers() []interface{} {
	return []interface{}{
		(*Destroy_Any)(nil),
		(*Destroy_Tree)(nil),
		(*Destroy_Animal)(nil),
	}
}

type Meme struct {
	Id                   uint64   `protobuf:"varint,1,opt,name=id,proto3" json:"id,omitempty"`
	MemeName             string   `protobuf:"bytes,2,opt,name=meme_name,json=memeName,proto3" json:"meme_name,omitempty"`
	XXX_NoUnkeyedLiteral struct{} `json:"-"`
	XXX_unrecognized     []byte   `json:"-"`
	XXX_sizecache        int32    `json:"-"`
}

func (m *Meme) Reset()         { *m = Meme{} }
func (m *Meme) String() string { return proto.CompactTextString(m) }
func (*Meme) ProtoMessage()    {}
func (*Meme) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{9}
}

func (m *Meme) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_Meme.Unmarshal(m, b)
}
func (m *Meme) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_Meme.Marshal(b, m, deterministic)
}
func (m *Meme) XXX_Merge(src proto.Message) {
	xxx_messageInfo_Meme.Merge(m, src)
}
func (m *Meme) XXX_Size() int {
	return xxx_messageInfo_Meme.Size(m)
}
func (m *Meme) XXX_DiscardUnknown() {
	xxx_messageInfo_Meme.DiscardUnknown(m)
}

var xxx_messageInfo_Meme proto.InternalMessageInfo

func (m *Meme) GetId() uint64 {
	if m != nil {
		return m.Id
	}
	return 0
}

func (m *Meme) GetMemeName() string {
	if m != nil {
		return m.MemeName
	}
	return ""
}

// Client notifying being ready to handle gameplay
type Initialized struct {
	XXX_NoUnkeyedLiteral struct{} `json:"-"`
	XXX_unrecognized     []byte   `json:"-"`
	XXX_sizecache        int32    `json:"-"`
}

func (m *Initialized) Reset()         { *m = Initialized{} }
func (m *Initialized) String() string { return proto.CompactTextString(m) }
func (*Initialized) ProtoMessage()    {}
func (*Initialized) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{10}
}

func (m *Initialized) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_Initialized.Unmarshal(m, b)
}
func (m *Initialized) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_Initialized.Marshal(b, m, deterministic)
}
func (m *Initialized) XXX_Merge(src proto.Message) {
	xxx_messageInfo_Initialized.Merge(m, src)
}
func (m *Initialized) XXX_Size() int {
	return xxx_messageInfo_Initialized.Size(m)
}
func (m *Initialized) XXX_DiscardUnknown() {
	xxx_messageInfo_Initialized.DiscardUnknown(m)
}

var xxx_messageInfo_Initialized proto.InternalMessageInfo

type Transform struct {
	Id                   uint64                 `protobuf:"varint,1,opt,name=id,proto3" json:"id,omitempty"`
	Position             *vector3.Vector3       `protobuf:"bytes,2,opt,name=position,proto3" json:"position,omitempty"`
	Rotation             *quaternion.Quaternion `protobuf:"bytes,3,opt,name=rotation,proto3" json:"rotation,omitempty"`
	XXX_NoUnkeyedLiteral struct{}               `json:"-"`
	XXX_unrecognized     []byte                 `json:"-"`
	XXX_sizecache        int32                  `json:"-"`
}

func (m *Transform) Reset()         { *m = Transform{} }
func (m *Transform) String() string { return proto.CompactTextString(m) }
func (*Transform) ProtoMessage()    {}
func (*Transform) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{11}
}

func (m *Transform) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_Transform.Unmarshal(m, b)
}
func (m *Transform) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_Transform.Marshal(b, m, deterministic)
}
func (m *Transform) XXX_Merge(src proto.Message) {
	xxx_messageInfo_Transform.Merge(m, src)
}
func (m *Transform) XXX_Size() int {
	return xxx_messageInfo_Transform.Size(m)
}
func (m *Transform) XXX_DiscardUnknown() {
	xxx_messageInfo_Transform.DiscardUnknown(m)
}

var xxx_messageInfo_Transform proto.InternalMessageInfo

func (m *Transform) GetId() uint64 {
	if m != nil {
		return m.Id
	}
	return 0
}

func (m *Transform) GetPosition() *vector3.Vector3 {
	if m != nil {
		return m.Position
	}
	return nil
}

func (m *Transform) GetRotation() *quaternion.Quaternion {
	if m != nil {
		return m.Rotation
	}
	return nil
}

type Tree struct {
	Transform            *Transform `protobuf:"bytes,1,opt,name=transform,proto3" json:"transform,omitempty"`
	XXX_NoUnkeyedLiteral struct{}   `json:"-"`
	XXX_unrecognized     []byte     `json:"-"`
	XXX_sizecache        int32      `json:"-"`
}

func (m *Tree) Reset()         { *m = Tree{} }
func (m *Tree) String() string { return proto.CompactTextString(m) }
func (*Tree) ProtoMessage()    {}
func (*Tree) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{12}
}

func (m *Tree) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_Tree.Unmarshal(m, b)
}
func (m *Tree) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_Tree.Marshal(b, m, deterministic)
}
func (m *Tree) XXX_Merge(src proto.Message) {
	xxx_messageInfo_Tree.Merge(m, src)
}
func (m *Tree) XXX_Size() int {
	return xxx_messageInfo_Tree.Size(m)
}
func (m *Tree) XXX_DiscardUnknown() {
	xxx_messageInfo_Tree.DiscardUnknown(m)
}

var xxx_messageInfo_Tree proto.InternalMessageInfo

func (m *Tree) GetTransform() *Transform {
	if m != nil {
		return m.Transform
	}
	return nil
}

type Animal struct {
	Transform            *Transform `protobuf:"bytes,1,opt,name=transform,proto3" json:"transform,omitempty"`
	XXX_NoUnkeyedLiteral struct{}   `json:"-"`
	XXX_unrecognized     []byte     `json:"-"`
	XXX_sizecache        int32      `json:"-"`
}

func (m *Animal) Reset()         { *m = Animal{} }
func (m *Animal) String() string { return proto.CompactTextString(m) }
func (*Animal) ProtoMessage()    {}
func (*Animal) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{13}
}

func (m *Animal) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_Animal.Unmarshal(m, b)
}
func (m *Animal) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_Animal.Marshal(b, m, deterministic)
}
func (m *Animal) XXX_Merge(src proto.Message) {
	xxx_messageInfo_Animal.Merge(m, src)
}
func (m *Animal) XXX_Size() int {
	return xxx_messageInfo_Animal.Size(m)
}
func (m *Animal) XXX_DiscardUnknown() {
	xxx_messageInfo_Animal.DiscardUnknown(m)
}

var xxx_messageInfo_Animal proto.InternalMessageInfo

func (m *Animal) GetTransform() *Transform {
	if m != nil {
		return m.Transform
	}
	return nil
}

// Ask a logic server to transfer ownership of an object
type RequestTransferOwnership struct {
	Transform            *Transform `protobuf:"bytes,1,opt,name=transform,proto3" json:"transform,omitempty"`
	XXX_NoUnkeyedLiteral struct{}   `json:"-"`
	XXX_unrecognized     []byte     `json:"-"`
	XXX_sizecache        int32      `json:"-"`
}

func (m *RequestTransferOwnership) Reset()         { *m = RequestTransferOwnership{} }
func (m *RequestTransferOwnership) String() string { return proto.CompactTextString(m) }
func (*RequestTransferOwnership) ProtoMessage()    {}
func (*RequestTransferOwnership) Descriptor() ([]byte, []int) {
	return fileDescriptor_dcbdca058206953b, []int{14}
}

func (m *RequestTransferOwnership) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_RequestTransferOwnership.Unmarshal(m, b)
}
func (m *RequestTransferOwnership) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_RequestTransferOwnership.Marshal(b, m, deterministic)
}
func (m *RequestTransferOwnership) XXX_Merge(src proto.Message) {
	xxx_messageInfo_RequestTransferOwnership.Merge(m, src)
}
func (m *RequestTransferOwnership) XXX_Size() int {
	return xxx_messageInfo_RequestTransferOwnership.Size(m)
}
func (m *RequestTransferOwnership) XXX_DiscardUnknown() {
	xxx_messageInfo_RequestTransferOwnership.DiscardUnknown(m)
}

var xxx_messageInfo_RequestTransferOwnership proto.InternalMessageInfo

func (m *RequestTransferOwnership) GetTransform() *Transform {
	if m != nil {
		return m.Transform
	}
	return nil
}

func init() {
	proto.RegisterType((*Packet)(nil), "nakama.niwrad.api.realtime.Packet")
	proto.RegisterType((*MatchJoin)(nil), "nakama.niwrad.api.realtime.MatchJoin")
	proto.RegisterType((*Map)(nil), "nakama.niwrad.api.realtime.Map")
	proto.RegisterType((*Matrix)(nil), "nakama.niwrad.api.realtime.Matrix")
	proto.RegisterType((*Array)(nil), "nakama.niwrad.api.realtime.Array")
	proto.RegisterType((*UpdateTransform)(nil), "nakama.niwrad.api.realtime.UpdateTransform")
	proto.RegisterType((*NavMeshUpdate)(nil), "nakama.niwrad.api.realtime.NavMeshUpdate")
	proto.RegisterType((*Spawn)(nil), "nakama.niwrad.api.realtime.Spawn")
	proto.RegisterType((*Destroy)(nil), "nakama.niwrad.api.realtime.Destroy")
	proto.RegisterType((*Meme)(nil), "nakama.niwrad.api.realtime.Meme")
	proto.RegisterType((*Initialized)(nil), "nakama.niwrad.api.realtime.Initialized")
	proto.RegisterType((*Transform)(nil), "nakama.niwrad.api.realtime.Transform")
	proto.RegisterType((*Tree)(nil), "nakama.niwrad.api.realtime.Tree")
	proto.RegisterType((*Animal)(nil), "nakama.niwrad.api.realtime.Animal")
	proto.RegisterType((*RequestTransferOwnership)(nil), "nakama.niwrad.api.realtime.RequestTransferOwnership")
}

func init() {
	proto.RegisterFile("realtime.proto", fileDescriptor_dcbdca058206953b)
}

var fileDescriptor_dcbdca058206953b = []byte{
	// 874 bytes of a gzipped FileDescriptorProto
	0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0xff, 0xd4, 0x56, 0xe1, 0x6e, 0x1b, 0x45,
	0x17, 0xf5, 0x7a, 0xed, 0x8d, 0xf7, 0xba, 0x89, 0xf3, 0xcd, 0x27, 0xaa, 0x25, 0x91, 0xa8, 0xd9,
	0x0a, 0x11, 0x84, 0xb4, 0x2e, 0x75, 0x0b, 0x8a, 0x4a, 0x55, 0xc5, 0x20, 0xb4, 0xa5, 0x4a, 0x0a,
	0x13, 0xa8, 0x10, 0x3f, 0x58, 0x4d, 0xbc, 0x97, 0x7a, 0xa8, 0x77, 0x76, 0x3b, 0x33, 0x4e, 0x62,
	0x5e, 0x83, 0xb7, 0xe0, 0x29, 0x10, 0x0f, 0xc3, 0x73, 0xa0, 0x9d, 0xd9, 0xb5, 0x9d, 0x22, 0x1c,
	0x57, 0xca, 0x1f, 0x7e, 0xcd, 0xdc, 0x3b, 0xe7, 0x9c, 0x19, 0xcf, 0xdc, 0x73, 0xbd, 0xb0, 0x23,
	0x91, 0x4d, 0x35, 0xcf, 0x30, 0x2a, 0x64, 0xae, 0x73, 0xb2, 0x27, 0xd8, 0x2b, 0x96, 0xb1, 0x48,
	0xf0, 0x0b, 0xc9, 0xd2, 0x88, 0x15, 0x3c, 0xaa, 0x11, 0x7b, 0x8f, 0x5e, 0x72, 0x3d, 0x99, 0x9d,
	0x45, 0xe3, 0x3c, 0x1b, 0x4c, 0xf3, 0x19, 0x57, 0xf7, 0x86, 0xf7, 0x3e, 0x39, 0x7c, 0x38, 0x30,
	0xcc, 0x0c, 0xb5, 0x9c, 0x0f, 0x58, 0xc1, 0x07, 0xe7, 0x38, 0xd6, 0xb9, 0x1c, 0xd6, 0xa3, 0x15,
	0xde, 0x3b, 0xda, 0x8c, 0xfc, 0x7a, 0xc6, 0x34, 0x4a, 0xc1, 0x73, 0xb1, 0x32, 0xad, 0x24, 0x0e,
	0x37, 0xdc, 0x3f, 0x9f, 0xce, 0x32, 0xac, 0x06, 0x4b, 0x0d, 0xff, 0xf2, 0xc0, 0xfb, 0x86, 0x8d,
	0x5f, 0xa1, 0x26, 0xfb, 0xe0, 0x2b, 0x14, 0x29, 0xca, 0x84, 0xa7, 0x81, 0xd3, 0x77, 0x0e, 0x7c,
	0xda, 0xb1, 0x89, 0xa7, 0x69, 0xb9, 0xc8, 0x55, 0xa2, 0x50, 0x9e, 0xa3, 0x0c, 0x9a, 0x7d, 0xe7,
	0xa0, 0x43, 0x3b, 0x5c, 0x9d, 0x9a, 0x98, 0xbc, 0x07, 0x20, 0x71, 0xcc, 0x0b, 0x8e, 0x42, 0xab,
	0xc0, 0xed, 0xbb, 0x07, 0x3e, 0x5d, 0xc9, 0x90, 0x21, 0x78, 0x3c, 0x2b, 0xd8, 0x58, 0x07, 0xad,
	0xbe, 0x73, 0xd0, 0xbd, 0xbf, 0x1f, 0x2d, 0x4f, 0x16, 0xd5, 0xb7, 0xf1, 0xc2, 0x8e, 0xb4, 0x82,
	0x92, 0xaf, 0x00, 0x32, 0xa6, 0xc7, 0x93, 0xe4, 0x97, 0x9c, 0x8b, 0x60, 0xcb, 0x10, 0x3f, 0x88,
	0xfe, 0xfd, 0x15, 0xa2, 0xe3, 0x12, 0xfd, 0x75, 0xce, 0x45, 0xdc, 0xa0, 0x7e, 0x56, 0x07, 0x64,
	0x08, 0x6e, 0xc6, 0x8a, 0xa0, 0x63, 0x04, 0xee, 0xac, 0x17, 0x28, 0xe2, 0x06, 0x2d, 0xd1, 0xe4,
	0x07, 0xd8, 0x9d, 0x15, 0x29, 0xd3, 0x98, 0x68, 0xc9, 0x84, 0xfa, 0x39, 0x97, 0x59, 0x00, 0x46,
	0xe1, 0xe3, 0x75, 0x0a, 0xdf, 0x1b, 0xce, 0x77, 0x35, 0x25, 0x6e, 0xd0, 0xde, 0xec, 0x6a, 0x8a,
	0x9c, 0x42, 0x4f, 0xb0, 0xf3, 0x24, 0x43, 0x35, 0x49, 0xec, 0x5a, 0xd0, 0x35, 0xc2, 0x1f, 0xad,
	0x13, 0x3e, 0x61, 0xe7, 0xc7, 0xa8, 0x26, 0x56, 0x3f, 0x6e, 0xd0, 0x6d, 0xb1, 0x9a, 0x20, 0x87,
	0xd0, 0x56, 0x05, 0xbb, 0x10, 0x41, 0xcf, 0x48, 0xbd, 0xbf, 0x4e, 0xea, 0xb4, 0x04, 0xc6, 0x0d,
	0x6a, 0x19, 0x24, 0x86, 0x6d, 0x89, 0xaf, 0x67, 0xa8, 0x74, 0x62, 0x25, 0x76, 0x37, 0x97, 0xb8,
	0x55, 0x31, 0x4d, 0x4c, 0x9e, 0xc0, 0x56, 0x8a, 0x4a, 0xcb, 0x7c, 0x1e, 0xfc, 0xcf, 0x68, 0xdc,
	0x5d, 0xa7, 0xf1, 0xa5, 0x85, 0xc6, 0x0d, 0x5a, 0xb3, 0xc8, 0x09, 0xf4, 0xea, 0xa3, 0xd4, 0x42,
	0xe4, 0x6d, 0x84, 0x76, 0x2a, 0x76, 0x95, 0x21, 0x9f, 0x42, 0x2b, 0xc3, 0x0c, 0x83, 0xff, 0x1b,
	0x91, 0xfe, 0xda, 0xa7, 0xc7, 0xac, 0xbc, 0x56, 0x83, 0x27, 0xcf, 0xa0, 0xcb, 0x05, 0xd7, 0x9c,
	0x4d, 0xf9, 0xaf, 0x98, 0x06, 0xef, 0x1a, 0xfa, 0x87, 0xeb, 0xe8, 0x4f, 0x97, 0xf0, 0xb8, 0x41,
	0x57, 0xd9, 0x23, 0x0f, 0x5a, 0x7a, 0x5e, 0x60, 0xf8, 0x1c, 0xfc, 0x45, 0x81, 0x92, 0x08, 0x3c,
	0x89, 0x2f, 0x79, 0x2e, 0x8c, 0xcf, 0xba, 0xf7, 0x6f, 0x5f, 0x31, 0x84, 0xf5, 0xe7, 0x28, 0xbf,
	0xa4, 0x15, 0x8a, 0x10, 0x68, 0x29, 0xc4, 0xd4, 0x18, 0xcf, 0xa5, 0x66, 0x1e, 0x72, 0x70, 0x8f,
	0x59, 0x41, 0xde, 0x01, 0xef, 0x32, 0x39, 0x63, 0x0a, 0x8d, 0x94, 0x4b, 0xdb, 0x97, 0x23, 0xa6,
	0xb0, 0x4c, 0xcf, 0x6d, 0xda, 0x72, 0xda, 0x73, 0x93, 0x7e, 0x60, 0xcd, 0xe0, 0x9a, 0x5d, 0xc3,
	0x6b, 0xdc, 0x24, 0xf9, 0xa5, 0x71, 0x43, 0xf8, 0x04, 0x3c, 0x1b, 0x92, 0x87, 0xd0, 0x92, 0xf9,
	0x85, 0x0a, 0x9c, 0xbe, 0x7b, 0x5d, 0x91, 0x1c, 0x49, 0xc9, 0xe6, 0xd4, 0xc0, 0xc3, 0x3b, 0xd0,
	0x36, 0x21, 0xb9, 0x0d, 0xad, 0x71, 0x3e, 0xb5, 0x7c, 0x67, 0xd4, 0xdc, 0x75, 0xa8, 0x89, 0xc3,
	0x17, 0xd0, 0x7b, 0xc3, 0x3b, 0xe4, 0x0b, 0xf0, 0x97, 0xde, 0x73, 0xae, 0xb7, 0xff, 0x82, 0x49,
	0x97, 0xbc, 0xf0, 0x27, 0xd8, 0xbe, 0x62, 0x1d, 0xb2, 0x03, 0xcd, 0xaa, 0xbb, 0xb5, 0x68, 0x93,
	0xa7, 0xe4, 0x31, 0x74, 0xcb, 0x5a, 0xe3, 0x82, 0xe9, 0xf2, 0x39, 0x9a, 0xd7, 0xf7, 0xa7, 0x55,
	0x7c, 0xf8, 0x87, 0x03, 0x6d, 0x5b, 0xfd, 0x87, 0xe0, 0x32, 0x31, 0x7f, 0xab, 0x83, 0x96, 0xcd,
	0x86, 0x09, 0x53, 0xa7, 0x5a, 0x22, 0x56, 0x9b, 0xf7, 0xd7, 0x73, 0xd1, 0xd4, 0x69, 0x89, 0x27,
	0x9f, 0x83, 0xc7, 0x04, 0xcf, 0xd8, 0x74, 0x93, 0xf7, 0x3c, 0x32, 0xc8, 0xb8, 0x41, 0x2b, 0xce,
	0xa2, 0x30, 0xff, 0x74, 0x60, 0xab, 0x76, 0xcc, 0x7f, 0xf6, 0x47, 0x0c, 0xa1, 0x55, 0x5a, 0xf8,
	0x1f, 0xcf, 0xbb, 0x0f, 0x7e, 0x69, 0xe9, 0x44, 0xb0, 0xcc, 0x1e, 0xcd, 0xa7, 0x9d, 0x32, 0x71,
	0xc2, 0x32, 0x0c, 0xb7, 0xa1, 0xbb, 0x62, 0xdc, 0xf0, 0x37, 0x07, 0xfc, 0x65, 0xf9, 0xbd, 0xa9,
	0xf4, 0x19, 0x74, 0x8a, 0x5c, 0xf1, 0x4d, 0xab, 0x64, 0x01, 0x26, 0x8f, 0xa1, 0x23, 0x73, 0x6d,
	0xcb, 0xcb, 0xad, 0x7a, 0xeb, 0x0a, 0x71, 0xe5, 0xcf, 0xfc, 0xdb, 0xc5, 0x94, 0x2e, 0x28, 0xe1,
	0x33, 0x68, 0x95, 0xf7, 0x75, 0x33, 0x76, 0x38, 0x06, 0xcf, 0x5e, 0xe1, 0xcd, 0xc8, 0x25, 0x10,
	0x50, 0xdb, 0x72, 0xed, 0x32, 0xca, 0xe7, 0x17, 0x02, 0xa5, 0x9a, 0xf0, 0xe2, 0x46, 0x36, 0x18,
	0x3d, 0xf8, 0xf1, 0xae, 0xa5, 0x0c, 0x2c, 0xc5, 0x7c, 0xc6, 0xd4, 0x94, 0x47, 0xf5, 0xe4, 0xf7,
	0xe6, 0xad, 0xa3, 0x82, 0x47, 0xb4, 0x0a, 0xcf, 0x3c, 0x73, 0xbd, 0xc3, 0xbf, 0x03, 0x00, 0x00,
	0xff, 0xff, 0xda, 0x86, 0x3a, 0x32, 0xc3, 0x09, 0x00, 0x00,
}