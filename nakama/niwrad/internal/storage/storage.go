package storage

import (
    "context"
    "encoding/json"
    "github.com/golang/protobuf/jsonpb"
    "github.com/heroiclabs/nakama-common/api"
    "github.com/heroiclabs/nakama-common/runtime"
)

var (
	errGetAccount = runtime.NewError("cannot find account", 14)
)

type User struct {
    api.User
    MatchesOwned []string
}

// GetUsers retrieves users from database and return them
func GetUsers(ctx context.Context, nk runtime.NakamaModule, usersID ...string) ([]User, error) {
	var users []*api.User
	var err error
	// I don't know if it's possible to have err == nil and len == 0
	if users, err = nk.UsersGetId(ctx, usersID); err != nil || len(users) == 0 {
		return nil, errGetAccount
	}
	var objectIds []*runtime.StorageRead
	for _, user := range users {
		objectIds = append(objectIds, &runtime.StorageRead{
			Collection: "user",
			Key:        user.Id,
			UserID: user.Id,
		})
	}

	// We need to read our custom stored users
	objects, err := nk.StorageRead(ctx, objectIds)
	if err != nil {
		return nil, errGetAccount
	}

    var usersStorage []User
	for i, o := range objects {
		var user User
		if err = jsonpb.UnmarshalString(o.Value, &user); err != nil {
			return nil, err
		}
		user.User = *users[i]
		usersStorage = append(usersStorage, user)
	}

    return usersStorage, nil
}

// Retrieves servers in database and return them
func GetMatches(ctx context.Context, nk runtime.NakamaModule, matchesID ...string) ([]api.Match, error) {
	var objectIds []*runtime.StorageRead
	for _, matchID := range matchesID {
		objectIds = append(objectIds, &runtime.StorageRead{
			Collection: "match",
			Key:        matchID,
		})
	}

	objects, err := nk.StorageRead(ctx, objectIds)
	if err != nil {
		return nil, err
	}
	var matches []api.Match
	for _, o := range objects {
		var match api.Match
		if err = jsonpb.UnmarshalString(o.Value, &match); err != nil {
			return nil, err
		}
        matches = append(matches, match)
	}
	return matches, nil
}

func UpdateUser(ctx context.Context, nk runtime.NakamaModule, userID string, matchesOwned []string) error {
	user := User{MatchesOwned: matchesOwned}
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

func UpdateMatch(ctx context.Context, nk runtime.NakamaModule, matchID string, userID string) error {
	match := api.Match{MatchId: matchID}
	jsonMatch, err := json.Marshal(match)
	if err != nil {
		return err
	}
	objects := []*runtime.StorageWrite{
		{
			Collection:      "match",
			Key:             matchID,
			UserID:          userID,
			Value:           string(jsonMatch),
			PermissionRead:  2,
			PermissionWrite: 1,
		},
	}

	if _, err := nk.StorageWrite(ctx, objects); err != nil {
		return err
	}
	return nil
}

func DeleteMatch(ctx context.Context, nk runtime.NakamaModule, matchID string, userID string) error {
	objects := []*runtime.StorageDelete{
		{
			Collection: "match",
			Key:        matchID,
			UserID:     userID,
		},
	}
	// Delete match from storage
	if err := nk.StorageDelete(ctx, objects); err != nil {
		return err
	}
	return nil
}
