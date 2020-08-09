// Code generated by protoc-gen-go. DO NOT EDIT.
// source: rpc.proto

package rpc

import (
	fmt "fmt"
	proto "github.com/golang/protobuf/proto"
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

type CreateMatchCompletionResult int32

const (
	CreateMatchCompletionResult_createMatchCompletionResultUnknownInternalFailure        CreateMatchCompletionResult = 0
	CreateMatchCompletionResult_createMatchCompletionResultSucceeded                     CreateMatchCompletionResult = 1
	CreateMatchCompletionResult_createMatchCompletionResultUnknownClientFailure          CreateMatchCompletionResult = -100
	CreateMatchCompletionResult_createMatchCompletionResultAlreadyInMatchOfSpecifiedType CreateMatchCompletionResult = -99
	CreateMatchCompletionResult_createMatchCompletionResultAlreadyCreatingMatch          CreateMatchCompletionResult = -98
	CreateMatchCompletionResult_createMatchCompletionResultAlreadyInMatch                CreateMatchCompletionResult = -97
	CreateMatchCompletionResult_createMatchCompletionResultFailedToCreateMucRoom         CreateMatchCompletionResult = -96
	CreateMatchCompletionResult_createMatchCompletionResultNoResponse                    CreateMatchCompletionResult = -95
	CreateMatchCompletionResult_createMatchCompletionResultLoggedOut                     CreateMatchCompletionResult = -94
)

var CreateMatchCompletionResult_name = map[int32]string{
	0:    "createMatchCompletionResultUnknownInternalFailure",
	1:    "createMatchCompletionResultSucceeded",
	-100: "createMatchCompletionResultUnknownClientFailure",
	-99:  "createMatchCompletionResultAlreadyInMatchOfSpecifiedType",
	-98:  "createMatchCompletionResultAlreadyCreatingMatch",
	-97:  "createMatchCompletionResultAlreadyInMatch",
	-96:  "createMatchCompletionResultFailedToCreateMucRoom",
	-95:  "createMatchCompletionResultNoResponse",
	-94:  "createMatchCompletionResultLoggedOut",
}

var CreateMatchCompletionResult_value = map[string]int32{
	"createMatchCompletionResultUnknownInternalFailure":        0,
	"createMatchCompletionResultSucceeded":                     1,
	"createMatchCompletionResultUnknownClientFailure":          -100,
	"createMatchCompletionResultAlreadyInMatchOfSpecifiedType": -99,
	"createMatchCompletionResultAlreadyCreatingMatch":          -98,
	"createMatchCompletionResultAlreadyInMatch":                -97,
	"createMatchCompletionResultFailedToCreateMucRoom":         -96,
	"createMatchCompletionResultNoResponse":                    -95,
	"createMatchCompletionResultLoggedOut":                     -94,
}

func (x CreateMatchCompletionResult) String() string {
	return proto.EnumName(CreateMatchCompletionResult_name, int32(x))
}

func (CreateMatchCompletionResult) EnumDescriptor() ([]byte, []int) {
	return fileDescriptor_77a6da22d6a3feb1, []int{0}
}

type RunServerCompletionResult int32

const (
	RunServerCompletionResult_runServerCompletionResultUnknownInternalFailure RunServerCompletionResult = 0
	RunServerCompletionResult_runServerCompletionResultSucceeded              RunServerCompletionResult = 1
)

var RunServerCompletionResult_name = map[int32]string{
	0: "runServerCompletionResultUnknownInternalFailure",
	1: "runServerCompletionResultSucceeded",
}

var RunServerCompletionResult_value = map[string]int32{
	"runServerCompletionResultUnknownInternalFailure": 0,
	"runServerCompletionResultSucceeded":              1,
}

func (x RunServerCompletionResult) String() string {
	return proto.EnumName(RunServerCompletionResult_name, int32(x))
}

func (RunServerCompletionResult) EnumDescriptor() ([]byte, []int) {
	return fileDescriptor_77a6da22d6a3feb1, []int{1}
}

type StopServerCompletionResult int32

const (
	StopServerCompletionResult_stopServerCompletionResultUnknownInternalFailure StopServerCompletionResult = 0
	StopServerCompletionResult_stopServerCompletionResultSucceeded              StopServerCompletionResult = 1
)

var StopServerCompletionResult_name = map[int32]string{
	0: "stopServerCompletionResultUnknownInternalFailure",
	1: "stopServerCompletionResultSucceeded",
}

var StopServerCompletionResult_value = map[string]int32{
	"stopServerCompletionResultUnknownInternalFailure": 0,
	"stopServerCompletionResultSucceeded":              1,
}

func (x StopServerCompletionResult) String() string {
	return proto.EnumName(StopServerCompletionResult_name, int32(x))
}

func (StopServerCompletionResult) EnumDescriptor() ([]byte, []int) {
	return fileDescriptor_77a6da22d6a3feb1, []int{2}
}

type CreateMatchRequest struct {
	WorkerId             string              `protobuf:"bytes,1,opt,name=worker_id,json=workerId,proto3" json:"worker_id,omitempty"`
	MatchType            string              `protobuf:"bytes,2,opt,name=match_type,json=matchType,proto3" json:"match_type,omitempty"`
	Seed                 int64               `protobuf:"varint,3,opt,name=seed,proto3" json:"seed,omitempty"`
	Configuration        *MatchConfiguration `protobuf:"bytes,4,opt,name=configuration,proto3" json:"configuration,omitempty"`
	XXX_NoUnkeyedLiteral struct{}            `json:"-"`
	XXX_unrecognized     []byte              `json:"-"`
	XXX_sizecache        int32               `json:"-"`
}

func (m *CreateMatchRequest) Reset()         { *m = CreateMatchRequest{} }
func (m *CreateMatchRequest) String() string { return proto.CompactTextString(m) }
func (*CreateMatchRequest) ProtoMessage()    {}
func (*CreateMatchRequest) Descriptor() ([]byte, []int) {
	return fileDescriptor_77a6da22d6a3feb1, []int{0}
}

func (m *CreateMatchRequest) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_CreateMatchRequest.Unmarshal(m, b)
}
func (m *CreateMatchRequest) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_CreateMatchRequest.Marshal(b, m, deterministic)
}
func (m *CreateMatchRequest) XXX_Merge(src proto.Message) {
	xxx_messageInfo_CreateMatchRequest.Merge(m, src)
}
func (m *CreateMatchRequest) XXX_Size() int {
	return xxx_messageInfo_CreateMatchRequest.Size(m)
}
func (m *CreateMatchRequest) XXX_DiscardUnknown() {
	xxx_messageInfo_CreateMatchRequest.DiscardUnknown(m)
}

var xxx_messageInfo_CreateMatchRequest proto.InternalMessageInfo

func (m *CreateMatchRequest) GetWorkerId() string {
	if m != nil {
		return m.WorkerId
	}
	return ""
}

func (m *CreateMatchRequest) GetMatchType() string {
	if m != nil {
		return m.MatchType
	}
	return ""
}

func (m *CreateMatchRequest) GetSeed() int64 {
	if m != nil {
		return m.Seed
	}
	return 0
}

func (m *CreateMatchRequest) GetConfiguration() *MatchConfiguration {
	if m != nil {
		return m.Configuration
	}
	return nil
}

type CreateMatchResponse struct {
	MatchId              string                      `protobuf:"bytes,1,opt,name=match_id,json=matchId,proto3" json:"match_id,omitempty"`
	Result               CreateMatchCompletionResult `protobuf:"varint,2,opt,name=result,proto3,enum=nakama.niwrad.api.rpc.CreateMatchCompletionResult" json:"result,omitempty"`
	XXX_NoUnkeyedLiteral struct{}                    `json:"-"`
	XXX_unrecognized     []byte                      `json:"-"`
	XXX_sizecache        int32                       `json:"-"`
}

func (m *CreateMatchResponse) Reset()         { *m = CreateMatchResponse{} }
func (m *CreateMatchResponse) String() string { return proto.CompactTextString(m) }
func (*CreateMatchResponse) ProtoMessage()    {}
func (*CreateMatchResponse) Descriptor() ([]byte, []int) {
	return fileDescriptor_77a6da22d6a3feb1, []int{1}
}

func (m *CreateMatchResponse) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_CreateMatchResponse.Unmarshal(m, b)
}
func (m *CreateMatchResponse) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_CreateMatchResponse.Marshal(b, m, deterministic)
}
func (m *CreateMatchResponse) XXX_Merge(src proto.Message) {
	xxx_messageInfo_CreateMatchResponse.Merge(m, src)
}
func (m *CreateMatchResponse) XXX_Size() int {
	return xxx_messageInfo_CreateMatchResponse.Size(m)
}
func (m *CreateMatchResponse) XXX_DiscardUnknown() {
	xxx_messageInfo_CreateMatchResponse.DiscardUnknown(m)
}

var xxx_messageInfo_CreateMatchResponse proto.InternalMessageInfo

func (m *CreateMatchResponse) GetMatchId() string {
	if m != nil {
		return m.MatchId
	}
	return ""
}

func (m *CreateMatchResponse) GetResult() CreateMatchCompletionResult {
	if m != nil {
		return m.Result
	}
	return CreateMatchCompletionResult_createMatchCompletionResultUnknownInternalFailure
}

type MatchConfiguration struct {
	TerrainSize          int32    `protobuf:"varint,1,opt,name=terrain_size,json=terrainSize,proto3" json:"terrain_size,omitempty"`
	InitialAnimals       int32    `protobuf:"varint,2,opt,name=initial_animals,json=initialAnimals,proto3" json:"initial_animals,omitempty"`
	InitialPlants        int32    `protobuf:"varint,3,opt,name=initial_plants,json=initialPlants,proto3" json:"initial_plants,omitempty"`
	XXX_NoUnkeyedLiteral struct{} `json:"-"`
	XXX_unrecognized     []byte   `json:"-"`
	XXX_sizecache        int32    `json:"-"`
}

func (m *MatchConfiguration) Reset()         { *m = MatchConfiguration{} }
func (m *MatchConfiguration) String() string { return proto.CompactTextString(m) }
func (*MatchConfiguration) ProtoMessage()    {}
func (*MatchConfiguration) Descriptor() ([]byte, []int) {
	return fileDescriptor_77a6da22d6a3feb1, []int{2}
}

func (m *MatchConfiguration) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_MatchConfiguration.Unmarshal(m, b)
}
func (m *MatchConfiguration) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_MatchConfiguration.Marshal(b, m, deterministic)
}
func (m *MatchConfiguration) XXX_Merge(src proto.Message) {
	xxx_messageInfo_MatchConfiguration.Merge(m, src)
}
func (m *MatchConfiguration) XXX_Size() int {
	return xxx_messageInfo_MatchConfiguration.Size(m)
}
func (m *MatchConfiguration) XXX_DiscardUnknown() {
	xxx_messageInfo_MatchConfiguration.DiscardUnknown(m)
}

var xxx_messageInfo_MatchConfiguration proto.InternalMessageInfo

func (m *MatchConfiguration) GetTerrainSize() int32 {
	if m != nil {
		return m.TerrainSize
	}
	return 0
}

func (m *MatchConfiguration) GetInitialAnimals() int32 {
	if m != nil {
		return m.InitialAnimals
	}
	return 0
}

func (m *MatchConfiguration) GetInitialPlants() int32 {
	if m != nil {
		return m.InitialPlants
	}
	return 0
}

type RunServerRequest struct {
	Configuration        *MatchConfiguration `protobuf:"bytes,1,opt,name=configuration,proto3" json:"configuration,omitempty"`
	XXX_NoUnkeyedLiteral struct{}            `json:"-"`
	XXX_unrecognized     []byte              `json:"-"`
	XXX_sizecache        int32               `json:"-"`
}

func (m *RunServerRequest) Reset()         { *m = RunServerRequest{} }
func (m *RunServerRequest) String() string { return proto.CompactTextString(m) }
func (*RunServerRequest) ProtoMessage()    {}
func (*RunServerRequest) Descriptor() ([]byte, []int) {
	return fileDescriptor_77a6da22d6a3feb1, []int{3}
}

func (m *RunServerRequest) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_RunServerRequest.Unmarshal(m, b)
}
func (m *RunServerRequest) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_RunServerRequest.Marshal(b, m, deterministic)
}
func (m *RunServerRequest) XXX_Merge(src proto.Message) {
	xxx_messageInfo_RunServerRequest.Merge(m, src)
}
func (m *RunServerRequest) XXX_Size() int {
	return xxx_messageInfo_RunServerRequest.Size(m)
}
func (m *RunServerRequest) XXX_DiscardUnknown() {
	xxx_messageInfo_RunServerRequest.DiscardUnknown(m)
}

var xxx_messageInfo_RunServerRequest proto.InternalMessageInfo

func (m *RunServerRequest) GetConfiguration() *MatchConfiguration {
	if m != nil {
		return m.Configuration
	}
	return nil
}

type RunServerResponse struct {
	Result               RunServerCompletionResult `protobuf:"varint,1,opt,name=result,proto3,enum=nakama.niwrad.api.rpc.RunServerCompletionResult" json:"result,omitempty"`
	XXX_NoUnkeyedLiteral struct{}                  `json:"-"`
	XXX_unrecognized     []byte                    `json:"-"`
	XXX_sizecache        int32                     `json:"-"`
}

func (m *RunServerResponse) Reset()         { *m = RunServerResponse{} }
func (m *RunServerResponse) String() string { return proto.CompactTextString(m) }
func (*RunServerResponse) ProtoMessage()    {}
func (*RunServerResponse) Descriptor() ([]byte, []int) {
	return fileDescriptor_77a6da22d6a3feb1, []int{4}
}

func (m *RunServerResponse) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_RunServerResponse.Unmarshal(m, b)
}
func (m *RunServerResponse) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_RunServerResponse.Marshal(b, m, deterministic)
}
func (m *RunServerResponse) XXX_Merge(src proto.Message) {
	xxx_messageInfo_RunServerResponse.Merge(m, src)
}
func (m *RunServerResponse) XXX_Size() int {
	return xxx_messageInfo_RunServerResponse.Size(m)
}
func (m *RunServerResponse) XXX_DiscardUnknown() {
	xxx_messageInfo_RunServerResponse.DiscardUnknown(m)
}

var xxx_messageInfo_RunServerResponse proto.InternalMessageInfo

func (m *RunServerResponse) GetResult() RunServerCompletionResult {
	if m != nil {
		return m.Result
	}
	return RunServerCompletionResult_runServerCompletionResultUnknownInternalFailure
}

type StopServerRequest struct {
	MatchId              string   `protobuf:"bytes,1,opt,name=match_id,json=matchId,proto3" json:"match_id,omitempty"`
	XXX_NoUnkeyedLiteral struct{} `json:"-"`
	XXX_unrecognized     []byte   `json:"-"`
	XXX_sizecache        int32    `json:"-"`
}

func (m *StopServerRequest) Reset()         { *m = StopServerRequest{} }
func (m *StopServerRequest) String() string { return proto.CompactTextString(m) }
func (*StopServerRequest) ProtoMessage()    {}
func (*StopServerRequest) Descriptor() ([]byte, []int) {
	return fileDescriptor_77a6da22d6a3feb1, []int{5}
}

func (m *StopServerRequest) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_StopServerRequest.Unmarshal(m, b)
}
func (m *StopServerRequest) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_StopServerRequest.Marshal(b, m, deterministic)
}
func (m *StopServerRequest) XXX_Merge(src proto.Message) {
	xxx_messageInfo_StopServerRequest.Merge(m, src)
}
func (m *StopServerRequest) XXX_Size() int {
	return xxx_messageInfo_StopServerRequest.Size(m)
}
func (m *StopServerRequest) XXX_DiscardUnknown() {
	xxx_messageInfo_StopServerRequest.DiscardUnknown(m)
}

var xxx_messageInfo_StopServerRequest proto.InternalMessageInfo

func (m *StopServerRequest) GetMatchId() string {
	if m != nil {
		return m.MatchId
	}
	return ""
}

type StopServerResponse struct {
	Result               StopServerCompletionResult `protobuf:"varint,1,opt,name=result,proto3,enum=nakama.niwrad.api.rpc.StopServerCompletionResult" json:"result,omitempty"`
	XXX_NoUnkeyedLiteral struct{}                   `json:"-"`
	XXX_unrecognized     []byte                     `json:"-"`
	XXX_sizecache        int32                      `json:"-"`
}

func (m *StopServerResponse) Reset()         { *m = StopServerResponse{} }
func (m *StopServerResponse) String() string { return proto.CompactTextString(m) }
func (*StopServerResponse) ProtoMessage()    {}
func (*StopServerResponse) Descriptor() ([]byte, []int) {
	return fileDescriptor_77a6da22d6a3feb1, []int{6}
}

func (m *StopServerResponse) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_StopServerResponse.Unmarshal(m, b)
}
func (m *StopServerResponse) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_StopServerResponse.Marshal(b, m, deterministic)
}
func (m *StopServerResponse) XXX_Merge(src proto.Message) {
	xxx_messageInfo_StopServerResponse.Merge(m, src)
}
func (m *StopServerResponse) XXX_Size() int {
	return xxx_messageInfo_StopServerResponse.Size(m)
}
func (m *StopServerResponse) XXX_DiscardUnknown() {
	xxx_messageInfo_StopServerResponse.DiscardUnknown(m)
}

var xxx_messageInfo_StopServerResponse proto.InternalMessageInfo

func (m *StopServerResponse) GetResult() StopServerCompletionResult {
	if m != nil {
		return m.Result
	}
	return StopServerCompletionResult_stopServerCompletionResultUnknownInternalFailure
}

type UnityServer struct {
	WorkerId             string              `protobuf:"bytes,1,opt,name=worker_id,json=workerId,proto3" json:"worker_id,omitempty"`
	MatchId              string              `protobuf:"bytes,2,opt,name=match_id,json=matchId,proto3" json:"match_id,omitempty"`
	Configuration        *MatchConfiguration `protobuf:"bytes,3,opt,name=configuration,proto3" json:"configuration,omitempty"`
	XXX_NoUnkeyedLiteral struct{}            `json:"-"`
	XXX_unrecognized     []byte              `json:"-"`
	XXX_sizecache        int32               `json:"-"`
}

func (m *UnityServer) Reset()         { *m = UnityServer{} }
func (m *UnityServer) String() string { return proto.CompactTextString(m) }
func (*UnityServer) ProtoMessage()    {}
func (*UnityServer) Descriptor() ([]byte, []int) {
	return fileDescriptor_77a6da22d6a3feb1, []int{7}
}

func (m *UnityServer) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_UnityServer.Unmarshal(m, b)
}
func (m *UnityServer) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_UnityServer.Marshal(b, m, deterministic)
}
func (m *UnityServer) XXX_Merge(src proto.Message) {
	xxx_messageInfo_UnityServer.Merge(m, src)
}
func (m *UnityServer) XXX_Size() int {
	return xxx_messageInfo_UnityServer.Size(m)
}
func (m *UnityServer) XXX_DiscardUnknown() {
	xxx_messageInfo_UnityServer.DiscardUnknown(m)
}

var xxx_messageInfo_UnityServer proto.InternalMessageInfo

func (m *UnityServer) GetWorkerId() string {
	if m != nil {
		return m.WorkerId
	}
	return ""
}

func (m *UnityServer) GetMatchId() string {
	if m != nil {
		return m.MatchId
	}
	return ""
}

func (m *UnityServer) GetConfiguration() *MatchConfiguration {
	if m != nil {
		return m.Configuration
	}
	return nil
}

type User struct {
	MatchesOwned         []string `protobuf:"bytes,1,rep,name=matches_owned,json=matchesOwned,proto3" json:"matches_owned,omitempty"`
	XXX_NoUnkeyedLiteral struct{} `json:"-"`
	XXX_unrecognized     []byte   `json:"-"`
	XXX_sizecache        int32    `json:"-"`
}

func (m *User) Reset()         { *m = User{} }
func (m *User) String() string { return proto.CompactTextString(m) }
func (*User) ProtoMessage()    {}
func (*User) Descriptor() ([]byte, []int) {
	return fileDescriptor_77a6da22d6a3feb1, []int{8}
}

func (m *User) XXX_Unmarshal(b []byte) error {
	return xxx_messageInfo_User.Unmarshal(m, b)
}
func (m *User) XXX_Marshal(b []byte, deterministic bool) ([]byte, error) {
	return xxx_messageInfo_User.Marshal(b, m, deterministic)
}
func (m *User) XXX_Merge(src proto.Message) {
	xxx_messageInfo_User.Merge(m, src)
}
func (m *User) XXX_Size() int {
	return xxx_messageInfo_User.Size(m)
}
func (m *User) XXX_DiscardUnknown() {
	xxx_messageInfo_User.DiscardUnknown(m)
}

var xxx_messageInfo_User proto.InternalMessageInfo

func (m *User) GetMatchesOwned() []string {
	if m != nil {
		return m.MatchesOwned
	}
	return nil
}

func init() {
	proto.RegisterEnum("nakama.niwrad.api.rpc.CreateMatchCompletionResult", CreateMatchCompletionResult_name, CreateMatchCompletionResult_value)
	proto.RegisterEnum("nakama.niwrad.api.rpc.RunServerCompletionResult", RunServerCompletionResult_name, RunServerCompletionResult_value)
	proto.RegisterEnum("nakama.niwrad.api.rpc.StopServerCompletionResult", StopServerCompletionResult_name, StopServerCompletionResult_value)
	proto.RegisterType((*CreateMatchRequest)(nil), "nakama.niwrad.api.rpc.CreateMatchRequest")
	proto.RegisterType((*CreateMatchResponse)(nil), "nakama.niwrad.api.rpc.CreateMatchResponse")
	proto.RegisterType((*MatchConfiguration)(nil), "nakama.niwrad.api.rpc.MatchConfiguration")
	proto.RegisterType((*RunServerRequest)(nil), "nakama.niwrad.api.rpc.RunServerRequest")
	proto.RegisterType((*RunServerResponse)(nil), "nakama.niwrad.api.rpc.RunServerResponse")
	proto.RegisterType((*StopServerRequest)(nil), "nakama.niwrad.api.rpc.StopServerRequest")
	proto.RegisterType((*StopServerResponse)(nil), "nakama.niwrad.api.rpc.StopServerResponse")
	proto.RegisterType((*UnityServer)(nil), "nakama.niwrad.api.rpc.UnityServer")
	proto.RegisterType((*User)(nil), "nakama.niwrad.api.rpc.User")
}

func init() {
	proto.RegisterFile("rpc.proto", fileDescriptor_77a6da22d6a3feb1)
}

var fileDescriptor_77a6da22d6a3feb1 = []byte{
	// 681 bytes of a gzipped FileDescriptorProto
	0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0xff, 0xa4, 0x95, 0x5b, 0x6f, 0x13, 0x47,
	0x14, 0xc7, 0xbb, 0xb1, 0x73, 0xf1, 0xc9, 0xa5, 0x9b, 0xa9, 0x2a, 0x39, 0x8d, 0x2a, 0xa5, 0x4e,
	0xd3, 0x38, 0xa9, 0xb4, 0x4e, 0x9c, 0xb6, 0xaa, 0x04, 0x3c, 0x04, 0x0b, 0x84, 0x11, 0x60, 0xb4,
	0x4e, 0x5e, 0x90, 0x90, 0x35, 0xec, 0x9e, 0x98, 0x51, 0xd6, 0x33, 0xc3, 0xec, 0x2c, 0xc6, 0x81,
	0x77, 0xbe, 0x01, 0x4f, 0x40, 0x80, 0x47, 0xde, 0xf9, 0x7c, 0x20, 0x8f, 0xd7, 0x37, 0xc2, 0x2c,
	0x48, 0xf1, 0x93, 0xfd, 0x9f, 0xf9, 0x9f, 0xf3, 0xdb, 0x73, 0xf1, 0x42, 0x41, 0xc9, 0xc0, 0x93,
	0x4a, 0x68, 0x41, 0x7e, 0xe5, 0xf4, 0x94, 0x76, 0xa8, 0xc7, 0x59, 0x57, 0xd1, 0xd0, 0xa3, 0x92,
	0x79, 0x4a, 0x06, 0xa5, 0x4f, 0x0e, 0x90, 0x9a, 0x42, 0xaa, 0xf1, 0x2e, 0xd5, 0xc1, 0x63, 0x1f,
	0x9f, 0x24, 0x18, 0x6b, 0xb2, 0x0e, 0x85, 0xae, 0x50, 0xa7, 0xa8, 0x5a, 0x2c, 0x2c, 0x3a, 0x1b,
	0x4e, 0xb9, 0xe0, 0x2f, 0x0c, 0x84, 0x7a, 0x48, 0x7e, 0x07, 0xe8, 0xf4, 0x2f, 0xb7, 0x74, 0x4f,
	0x62, 0x71, 0xc6, 0x9c, 0x16, 0x8c, 0x72, 0xd4, 0x93, 0x48, 0x08, 0xe4, 0x63, 0xc4, 0xb0, 0x98,
	0xdb, 0x70, 0xca, 0x39, 0xdf, 0x7c, 0x27, 0x0d, 0x58, 0x0e, 0x04, 0x3f, 0x61, 0xed, 0x44, 0x51,
	0xcd, 0x04, 0x2f, 0xe6, 0x37, 0x9c, 0xf2, 0x62, 0x75, 0xc7, 0xfb, 0x26, 0x95, 0x67, 0x58, 0x6a,
	0x93, 0x06, 0x7f, 0xda, 0x5f, 0x7a, 0x01, 0xbf, 0x4c, 0x61, 0xc7, 0x52, 0xf0, 0x18, 0xc9, 0x1a,
	0x2c, 0x0c, 0xd0, 0x46, 0xd8, 0xf3, 0xe6, 0x77, 0x3d, 0x24, 0xb7, 0x61, 0x4e, 0x61, 0x9c, 0x44,
	0xda, 0x10, 0xaf, 0x54, 0xab, 0x96, 0xdc, 0x13, 0x61, 0x6b, 0xa2, 0x23, 0x23, 0x34, 0xe9, 0x8d,
	0xd3, 0x4f, 0x23, 0x94, 0x5e, 0x3a, 0x40, 0x2e, 0x32, 0x92, 0x3f, 0x60, 0x49, 0xa3, 0x52, 0x94,
	0xf1, 0x56, 0xcc, 0xce, 0xd0, 0x10, 0xcc, 0xfa, 0x8b, 0xa9, 0xd6, 0x64, 0x67, 0x48, 0xb6, 0xe1,
	0x67, 0xc6, 0x99, 0x66, 0x34, 0x6a, 0x51, 0xce, 0x3a, 0x34, 0x8a, 0x0d, 0xce, 0xac, 0xbf, 0x92,
	0xca, 0x87, 0x03, 0x95, 0x6c, 0xc1, 0x50, 0x69, 0xc9, 0x88, 0x72, 0x1d, 0x9b, 0x7a, 0xce, 0xfa,
	0xcb, 0xa9, 0x7a, 0xdf, 0x88, 0xa5, 0x00, 0x5c, 0x3f, 0xe1, 0x4d, 0x54, 0x4f, 0x51, 0x0d, 0x9b,
	0x77, 0xa1, 0xd8, 0xce, 0x25, 0x8b, 0xfd, 0x10, 0x56, 0x27, 0x92, 0xa4, 0xa5, 0xbe, 0x35, 0xaa,
	0xa7, 0x63, 0xea, 0xb9, 0x67, 0x09, 0x3f, 0x72, 0x5a, 0xab, 0xe9, 0xc1, 0x6a, 0x53, 0x0b, 0x39,
	0xfd, 0x10, 0xf6, 0x4e, 0x96, 0x5a, 0x40, 0x26, 0xef, 0xa7, 0x3c, 0xf5, 0xaf, 0x78, 0xf6, 0x2d,
	0x3c, 0x63, 0xab, 0x15, 0xe8, 0x95, 0x03, 0x8b, 0xc7, 0x9c, 0xe9, 0xde, 0xe0, 0x5e, 0xf6, 0x36,
	0x4c, 0x82, 0xce, 0x4c, 0x8f, 0xdc, 0x85, 0x46, 0xe4, 0x2e, 0xd9, 0x88, 0xbf, 0x21, 0x7f, 0x1c,
	0xa3, 0x22, 0x9b, 0xb0, 0x6c, 0x72, 0x60, 0xdc, 0x12, 0x5d, 0x8e, 0x7d, 0xa8, 0x5c, 0xb9, 0xe0,
	0x2f, 0xa5, 0x62, 0xa3, 0xaf, 0xed, 0x9e, 0xe7, 0x61, 0x3d, 0x63, 0x98, 0xc9, 0xbf, 0xb0, 0x1f,
	0xd8, 0x8f, 0x8f, 0xf9, 0x29, 0x17, 0x5d, 0x5e, 0xe7, 0x1a, 0x15, 0xa7, 0xd1, 0x4d, 0xca, 0xa2,
	0x44, 0xa1, 0xfb, 0x13, 0x29, 0xc3, 0x9f, 0x19, 0xb6, 0x66, 0x12, 0x04, 0x88, 0x21, 0x86, 0xae,
	0x43, 0xae, 0x42, 0xe5, 0xfb, 0x09, 0x6a, 0x11, 0x43, 0xae, 0x87, 0xe1, 0x5f, 0x7f, 0x4e, 0x3f,
	0x0e, 0xb9, 0x01, 0xff, 0x67, 0xb8, 0x0f, 0x23, 0x85, 0x34, 0xec, 0xd5, 0xb9, 0x39, 0x6d, 0x9c,
	0x34, 0x25, 0x06, 0xec, 0x84, 0x61, 0xd8, 0xff, 0x0b, 0x72, 0xdf, 0x8c, 0xc3, 0x64, 0x43, 0xa4,
	0x61, 0x4c, 0x99, 0x18, 0x6f, 0x9b, 0x3b, 0xee, 0xdb, 0xb1, 0xfb, 0x3f, 0xd8, 0xf9, 0x61, 0x08,
	0xf7, 0x7c, 0xec, 0xbb, 0x06, 0x7b, 0x19, 0xbe, 0xfe, 0xd3, 0x62, 0x78, 0x24, 0xd2, 0xee, 0x24,
	0x81, 0x2f, 0x44, 0xc7, 0x7d, 0x37, 0xb6, 0x57, 0x61, 0x2b, 0xc3, 0x7e, 0x4f, 0x0c, 0x87, 0xde,
	0x7d, 0x3f, 0xf6, 0xec, 0x67, 0xf6, 0xe5, 0x8e, 0x68, 0xb7, 0x31, 0x6c, 0x24, 0xda, 0xfd, 0x30,
	0xb2, 0xec, 0x3e, 0x83, 0x35, 0xeb, 0x76, 0x92, 0x03, 0xa8, 0x28, 0xdb, 0xa1, 0x75, 0x38, 0xfe,
	0x82, 0x92, 0xd5, 0x34, 0x31, 0x1a, 0xbb, 0xcf, 0xe1, 0x37, 0xfb, 0x1e, 0x92, 0x7f, 0x60, 0x2f,
	0xb6, 0x9e, 0x5a, 0x73, 0x6f, 0xc3, 0xa6, 0xdd, 0x35, 0x91, 0xfc, 0xfa, 0xd6, 0x83, 0xb5, 0xc1,
	0x02, 0x56, 0x06, 0x0b, 0x58, 0xa1, 0x92, 0x55, 0x94, 0x0c, 0xae, 0x28, 0x19, 0x7c, 0x9c, 0x99,
	0x3f, 0x94, 0xcc, 0xf3, 0x65, 0xf0, 0x68, 0xce, 0xbc, 0x38, 0x0f, 0xbe, 0x04, 0x00, 0x00, 0xff,
	0xff, 0x27, 0x31, 0xf7, 0xec, 0x45, 0x07, 0x00, 0x00,
}