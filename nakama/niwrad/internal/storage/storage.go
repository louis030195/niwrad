package storage

import (
	"context"
	"encoding/json"

	"github.com/golang/protobuf/jsonpb"
	"github.com/heroiclabs/nakama-common/api"
	"github.com/heroiclabs/nakama-common/runtime"
	"github.com/louis030195/niwrad/api/rpc"
)

var (
	errGetAccount = runtime.NewError("cannot find account", 14)
)

// GetUsers retrieves users from database and return them
func GetUsers(ctx context.Context, nk runtime.NakamaModule, usersID ...string) ([]*api.User, []rpc.User, error) {
	var users []*api.User
	var err error
	// I don't know if it's possible to have err == nil and len == 0
	if users, err = nk.UsersGetId(ctx, usersID); err != nil || len(users) == 0 {
		return nil, nil, errGetAccount
	}

	var objectIds []*runtime.StorageRead
	for _, userID := range usersID {
		objectIds = append(objectIds, &runtime.StorageRead{
			Collection: "user",
			Key:        userID,
		})
	}

	objects, err := nk.StorageRead(ctx, objectIds)
	if err != nil {
		return nil, nil, errGetAccount
	}
	var usersStorage []rpc.User
	for _, o := range objects {
		var user rpc.User
		if err = jsonpb.UnmarshalString(o.Value, &user); err != nil {
			return nil, nil, err
		}
		usersStorage = append(usersStorage, user)
	}
	return users, usersStorage, nil
}

// Retrieves servers in database and return them
func GetServers(ctx context.Context, nk runtime.NakamaModule, serversID ...string) ([]rpc.UnityServer, error) {
	var objectIds []*runtime.StorageRead
	for _, serverID := range serversID {
		objectIds = append(objectIds, &runtime.StorageRead{
			Collection: "server",
			Key:        serverID,
		})
	}

	objects, err := nk.StorageRead(ctx, objectIds)
	if err != nil {
		return nil, err
	}
	var servers []rpc.UnityServer
	for _, o := range objects {
		var server rpc.UnityServer
		if err = jsonpb.UnmarshalString(o.Value, &server); err != nil {
			return nil, err
		}
		servers = append(servers, server)
	}
	return servers, nil
}

func UpdateUser(ctx context.Context, nk runtime.NakamaModule, userID string, matchesOwned []string) error {
	user := rpc.User{MatchesOwned: matchesOwned}
	jsonUser, err := json.Marshal(user)
	if err != nil {
		return err
	}
	objects := []*runtime.StorageWrite{
		{
			Collection:      "user",
			Key:             userID,
			UserID:          userID,
			Value:           string(jsonUser),
			PermissionRead:  2,
			PermissionWrite: 1,
		},
	}

	if _, err := nk.StorageWrite(ctx, objects); err != nil {
		return err
	}
	return nil
}

func UpdateServer(ctx context.Context, nk runtime.NakamaModule, serverID string, userID string) error {
	server := rpc.UnityServer{MatchId: serverID}
	jsonServer, err := json.Marshal(server)
	if err != nil {
		return err
	}
	objects := []*runtime.StorageWrite{
		{
			Collection:      "server",
			Key:             serverID,
			UserID:          userID,
			Value:           string(jsonServer),
			PermissionRead:  2,
			PermissionWrite: 1,
		},
	}

	if _, err := nk.StorageWrite(ctx, objects); err != nil {
		return err
	}
	return nil
}

func DeleteServer(ctx context.Context, nk runtime.NakamaModule, matchID string, userID string) error {
	objects := []*runtime.StorageDelete{
		{
			Collection: "server",
			Key:        matchID,
			UserID:     userID,
		},
	}
	// Delete server from storage
	if err := nk.StorageDelete(ctx, objects); err != nil {
		return err
	}
	return nil
}
